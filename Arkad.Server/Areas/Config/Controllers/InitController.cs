using Arkad.Server.DAO.Impl;
using Arkad.Server.Models;
using Arkad.Shared;
using Hefesto.Encrypt;
using Hefesto.Response;
using Hefesto.Validation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Net;

namespace Arkad.Server.Areas.Config.Controllers
{
    [Area("config")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class InitController : ControllerBase
    {
        private string TAG = typeof(InitController).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration configuration;

        private string Authorization = "";
        private string clientSecret;

        public InitController(IConfiguration configuration)
        {
            this.configuration = configuration;
            clientSecret = configuration.GetSection("AppSettings").GetSection("ClientAuth").GetSection("Secret").Value;
        }

        [HttpGet("v1/setup")]
        [Produces("application/json")]
        //public async Task<ResponseGenericModel> Setup()
        public IActionResult Setup()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                #region DAO
                UserDao userDao = new UserDao();
                #endregion DAO

                #region DTO
                bool userActive = userDao.AnyActive();
                var roles = userDao.GetRoles(true);
                #endregion DTO

                //if (userActive)
                //{
                //    return Redirect("/auth/login");
                //}

                var vRoles = (roles is not null)
                                    ? roles.Adapt<List<Arkad.Shared.Models.Role>>()
                                    : null;
                var objectData = new
                {
                    any = userActive,
                    roles = vRoles
                };

                httpStatusCode = HttpStatusCode.OK;
                response.status = StatusResponseCodes.StatusResponseSuccess;
                response.subject = "Éxito";
                response.message = "Éxito";
                response.httpCode = (int)httpStatusCode;
                response.urlRedirect = "/";
                response.data = objectData;
            }
            catch (Exception e)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                response.status = StatusResponseCodes.StatusResponseError;
                response.subject = "No fue posible procesar la solicitud.";
                response.message = "Se produjo un error inesperado. Por favor intentelo nuevamente.";
                response.httpCode = (int)httpStatusCode;

                logger.Error($"{TAG}  -- {e}");
            }

            this.HttpContext.Response.StatusCode = httpStatusCode.GetHashCode();
            return StatusCode(httpStatusCode.GetHashCode(), response);
        }

        [HttpPost("v1/user/register")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> Register()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string clientIp = Request.Headers[TagUtil.TAG_REQUEST_HEADER_CLIENTIP].ToString();

                string name = HttpContext.Request.Form["name"];
                string email = HttpContext.Request.Form["email"];
                string emailrepeat = HttpContext.Request.Form["emailrepeat"];
                string password = Request.Form["password"];
                string passwordrepeat = Request.Form["passwordrepeat"];

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
                    if (String.IsNullOrEmpty(name))
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "El nombre de usuario ingresado no es valido";
                        response.message = "El nombre de usuario es obligatorio";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (String.IsNullOrEmpty(email) || !DataTypeValidation.checkEmailAddress(email))
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "El correo electrónico ingresado no es valido";
                        response.message = "El correo electrónico ingresado no tiene un formato valido";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (email != emailrepeat)
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = "El correo electrónico no coincide";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (String.IsNullOrEmpty(password))
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "La contraseña ingresada no es valida";
                        response.message = "La contraseña es obligatoria";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (password != passwordrepeat)
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = "La contraseña no coincide";
                        response.httpCode = (int)httpStatusCode;
                    }
                    #endregion VALIDACION
                    else
                    {
                        #region Formateo Datos
                        email = email.Trim().ToUpper();
                        emailrepeat = emailrepeat.Trim().ToUpper();
                        #endregion Formateo Datos

                        #region DAO
                        UserDao userDao = new UserDao();
                        #endregion DAO

                        #region DTO
                        User user = userDao.GetByEmail(email);
                        Role role = userDao.GetRoleByCode("SYSADMIN");
                        #endregion DTO

                        password = EncryptUtil.SHA512(password);

                        if (user != null && user.Active)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "El usuario ya existe!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else if (role is null)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No ha sido posible asignar un rol de usuario!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            bool success = false;

                            if (user != null && !user.Active)
                            {
                                user.Name = name;
                                user.Email = email;
                                user.Password = password;
                                user.ModifiedDate = DateTime.Now;
                                user.Active = true;
                                user.RoleId = role.Id;

                                success = userDao.Update(user);
                            }
                            else
                            {
                                user = new User();
                                user.Id = Guid.NewGuid().ToString();
                                user.Name = name;
                                user.Email = email;
                                user.Password = password;
                                user.CreatedDate = DateTime.Now;
                                user.ModifiedDate = DateTime.Now;
                                user.Active = true;
                                user.RoleId = role.Id;

                                success = userDao.Save(user);
                            }

                            if (success)
                            {
                                httpStatusCode = HttpStatusCode.OK;
                                response.status = StatusResponseCodes.StatusResponseSuccess;
                                response.subject = "Éxito";
                                response.message = "Usuario registrado con éxito";
                                response.httpCode = (int)httpStatusCode;
                            }
                            else
                            {
                                httpStatusCode = HttpStatusCode.InternalServerError;
                                response.status = StatusResponseCodes.StatusResponseError;
                                response.subject = "Error";
                                response.message = "No fue posible registrar el usuario. Por favor intentelo nuevamente!";
                                response.httpCode = (int)httpStatusCode;
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

                logger.Error($"{TAG}  -- {e}");
            }

            this.HttpContext.Response.StatusCode = httpStatusCode.GetHashCode();
            return response;
        }
    }
}
