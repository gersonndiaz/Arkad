﻿using Arkad.Server.DAO.Impl;
using Arkad.Server.Helpers;
using Arkad.Server.Models;
using Arkad.Shared;
using Hefesto.Encrypt;
using Hefesto.Response;
using Hefesto.Rut;
using Hefesto.Validation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Dynamic;
using System.Net;
using System.Security.Claims;
using UAParser;
using UAParser.Objects;

namespace Arkad.Server.Areas.Auth.Controllers
{
    [Area("auth")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private string TAG = typeof(AuthorizationController).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration configuration;

        private string Authorization = "";
        private string clientId;
        private string clientSecret;

        public AuthorizationController(IConfiguration configuration)
        {
            this.configuration = configuration;
            clientId = configuration.GetSection("AppSettings").GetSection("ClientAuth").GetSection("clientId").Value;
            clientSecret = configuration.GetSection("AppSettings").GetSection("ClientAuth").GetSection("clientSecret").Value;
        }

        [HttpPost("v1/login")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> Login()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string clientIp = Request.Headers[TagUtil.TAG_REQUEST_HEADER_CLIENTIP].ToString();

                string nombreUsuario = HttpContext.Request.Form["nombreUsuario"];
                string clave = Request.Form["clave"];


                if (!String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.OK;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Sesión actual existente";
                    response.message = "Usted ya tiene una sesión iniciada en este dispositivo.";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = (!String.IsNullOrEmpty(referer)) ? referer : "/";
                }
                else
                {
                    #region VALIDACION
                    if (String.IsNullOrEmpty(nombreUsuario))
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "El nombre de usuario ingresado no es valido";
                        response.message = "El nombre de usuario es obligatorio";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (!DataTypeValidation.checkEmailAddress(nombreUsuario)
                            && !Rut.validar(nombreUsuario))
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "El nombre de usuario ingresado no es valido";
                        response.message = "El nombre de usuario ingresado no tiene un formato valido";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (String.IsNullOrEmpty(clave))
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "La contraseña ingresada no es valida";
                        response.message = "La contraseña es obligatoria";
                        response.httpCode = (int)httpStatusCode;
                    }
                    #endregion VALIDACION
                    else
                    {
                        #region Formateo Datos
                        nombreUsuario = nombreUsuario.Trim().ToUpper();

                        if (Rut.validar(nombreUsuario))
                        {
                            nombreUsuario = Rut.removeFormatRutChile(nombreUsuario);
                            nombreUsuario.Trim().ToUpper();
                        }

                        #endregion Formateo Datos

                        #region DAO
                        UsuarioDao usuarioDao = new UsuarioDao();
                        #endregion DAO

                        #region DTO
                        Usuario usuario = (Rut.validar(nombreUsuario)) ? usuarioDao.GetByRun(nombreUsuario) : usuarioDao.GetByCorreo(nombreUsuario);
                        Rol rol = (usuario is not null) ? usuarioDao.GetRolById(usuario.RolId) : null;
                        #endregion DTO

                        clave = EncryptUtil.SHA512(clave);

                        if (usuario == null || !usuario.BActivo)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Acceso no autorizado!";
                            response.message = "Usuario no encontrado!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else if (usuario.Clave != clave)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Acceso no autorizado!";
                            response.message = "Usuario y/o contraseña incorrecta!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else if (rol is null)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Acceso no autorizado!";
                            response.message = "El usuario no tiene un rol asignado!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            usuario.FUltimaConexion = DateTime.Now;

                            Dictionary<string, object> objectData = new Dictionary<string, object>();

                            string userAgent = Request.Headers[TagUtil.TAG_REQUEST_HEADER_USERAGENT].ToString();
                            var uaParser = Parser.GetDefault();

                            ClientInfo c = uaParser.Parse(userAgent);

                            string dispositivo = $"{c.Device.Family}";
                            dispositivo += (!String.IsNullOrEmpty(c.Device.Brand)) ? $" {c.Device.Brand}" : "";
                            dispositivo += (!String.IsNullOrEmpty(c.Device.Model)) ? $" {c.Device.Model}" : "";

                            string sistemaOperativo = $"{c.OS.Family}";
                            sistemaOperativo += (!String.IsNullOrEmpty(c.OS.Major)) ? $" {c.OS.Major}" : "";
                            sistemaOperativo += (!String.IsNullOrEmpty(c.OS.Minor)) ? $".{c.OS.Minor}" : "";

                            string navegador = $"{c.Browser.Family}";
                            navegador += (!String.IsNullOrEmpty(c.Browser.Major)) ? $" {c.Browser.Major}" : "";
                            navegador += (!String.IsNullOrEmpty(c.Browser.Minor)) ? $".{c.Browser.Minor}" : "";

                            Claim[] claims = new[]
                                {
                                    new Claim(TagUtil.TAG_JWT_CLAIM_JWT_ID, Guid.NewGuid().ToString()),
                                    new Claim(TagUtil.TAG_JWT_CLAIM_USUARIO_ID, usuario.Id)
                                };

                            ClaimsIdentity ci = new ClaimsIdentity();
                            ci.AddClaims(claims);
                            Authorization = "Bearer " + SecurityUtils.TokenEncode(ci, null, clientSecret);

                            Dictionary<string, object> dataUsuario = new Dictionary<string, object>();

                            dynamic mUsuario = new ExpandoObject();
                            mUsuario.Nombre = usuario.Nombre;
                            mUsuario.Correo = usuario.Correo;
                            mUsuario.FUltimaConexion = usuario.FUltimaConexion;
                            mUsuario.Rol = rol.Codigo;

                            objectData.Add("usuario", mUsuario);

                            objectData.Add(TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION, Authorization);

                            bool success = usuarioDao.Update(usuario);

                            if (!success)
                            {
                                httpStatusCode = HttpStatusCode.InternalServerError;
                                response.status = StatusResponseCodes.StatusResponseError;
                                response.subject = "Error inicio sesión.";
                                response.message = "Se produjo un error inesperado al registrar su sesión. Por favor intentelo nuevamente.";
                                response.httpCode = (int)httpStatusCode;
                            }
                            else
                            {
                                httpStatusCode = HttpStatusCode.OK;
                                response.status = StatusResponseCodes.StatusResponseSuccess;
                                response.subject = "Acceso autorizado";
                                response.message = "Haz iniciado sesión con éxito";
                                response.httpCode = (int)httpStatusCode;
                                response.urlRedirect = "/";
                                response.data = objectData;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                response.status = StatusResponseCodes.StatusResponseError;
                response.subject = "No fue posible procesar la solicitud.";
                response.message = "Se produjo un error inesperado. Por favor intentelo nuevamente.";
                response.httpCode = (int)httpStatusCode;

                logger.Error(TAG + " -- " + e);
            }

            this.HttpContext.Response.StatusCode = httpStatusCode.GetHashCode();
            return response;
        }
    }
}
