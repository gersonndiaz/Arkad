using Arkad.Server.DAO.Impl;
using Arkad.Server.Helpers;
using Arkad.Server.Models;
using Arkad.Shared;
using Hefesto.Encrypt;
using Hefesto.Response;
using Hefesto.Validation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Net;
using System.Web;

namespace Arkad.Server.Areas.Account.Controllers
{
    [Area("account")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private string TAG = typeof(ProfileController).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration configuration;

        private string Authorization = "";
        private string clientId;
        private string clientSecret;

        public ProfileController(IConfiguration configuration)
        {
            this.configuration = configuration;
            clientSecret = configuration.GetSection("AppSettings").GetSection("ClientAuth").GetSection("Secret").Value;
        }

        [HttpGet("v1/get")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> Get()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region DAO
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    #endregion DTO

                    #region VALIDACION
                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error usuario";
                        response.message = "El usuario no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (role is null)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error rol";
                        response.message = "El rol de usuario no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    #endregion VALIDACION
                    else
                    {
                        var vUser = user.Adapt<Shared.Models.User>();
                        vUser.Password = null;

                        object data = new
                        {
                            user = vUser
                        };

                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseSuccess;
                        response.subject = "Éxito";
                        response.message = "Información cargada con éxito!";
                        response.httpCode = (int)httpStatusCode;
                        response.data = data;
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

        [HttpPost("v1/update")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> Update()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string password = Request.Form["password"];
                string name = Request.Form["name"];
                string email = Request.Form["email"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(password))
                {
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error contraseña";
                    response.message = "La contraseña del usuario es obligatoria!";
                }
                else if (String.IsNullOrEmpty(name))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error nombre";
                    response.message = "El nombre del usuario es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(email))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error correo";
                    response.message = "El correo del usuario es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkEmailAddress(email))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error correo";
                    response.message = "El correo del usuario no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    name = name.ToUpper().Trim();
                    email = email.ToUpper().Trim();

                    password = EncryptUtil.SHA512(password);

                    #region DAO
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    User user = userDao.GetById(userId);
                    User userAuxEmail = userDao.GetByEmail(email);
                    #endregion DTO

                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Acceso no autorizado!";
                        response.message = $"Su usuario no se encuentra autorizado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (!user.Password.Equals(password))
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error!";
                        response.message = $"La contraseña actual no es correcta!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (userAuxEmail is not null && user.Id != userAuxEmail.Id)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El usuario correo {userAuxEmail.Email} ya existe!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        var jUserCurrent = JsonConvert.SerializeObject(user);
                        var userCurrent = JsonConvert.DeserializeObject<User>(jUserCurrent);

                        user.Name = name;
                        user.Email = email;
                        user.ModifiedDate = DateTime.Now;

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}][Mis Datos]",
                            Description = $"Se ha actualizado el usuario:\n\n" +
                                          $"* Nombre: {userCurrent.Name} => {user.Name}\n" +
                                          $"* Correo: {userCurrent.Email}  =>  {user.Email}",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryUser historyUser = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(user),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            UserModifiedId = user.Id,
                            UserId = user.Id
                        };

                        bool success = userDao.Update(user, history, historyUser);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Usuario actualizado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar el usuario. Por favor intentelo nuevamente.";
                            response.httpCode = (int)httpStatusCode;
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

        [HttpPost("v1/password/update")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> UpdatePassword()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string password = Request.Form["password"];
                string passwordnew = Request.Form["passwordnew"];
                string passwordrepeat = Request.Form["passwordrepeat"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(password))
                {
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error contraseña";
                    response.message = "La contraseña del usuario es obligatoria!";
                }
                else if (String.IsNullOrEmpty(passwordnew))
                {
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error contraseña nueva";
                    response.message = "La contraseña nueva del usuario es obligatoria!";
                }
                else if (passwordnew.Length < 4)
                {
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error contraseña nueva";
                    response.message = "La contraseña nueva debe tener al menos 4 caracteres!";
                }
                else if (!passwordnew.Equals(passwordrepeat))
                {
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error contraseña";
                    response.message = "La contraseña de confirmación no coincide con la nueva contraseña ingresada!";
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    password = EncryptUtil.SHA512(password);
                    passwordnew = EncryptUtil.SHA512(passwordnew);

                    #region DAO
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    User user = userDao.GetById(userId);
                    #endregion DTO

                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Acceso no autorizado!";
                        response.message = $"Su usuario no se encuentra autorizado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (!user.Password.Equals(password))
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error!";
                        response.message = $"La contraseña actual no es correcta!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        user.Password = passwordnew;
                        user.ModifiedDate = DateTime.Now;

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}][Mis Datos]",
                            Description = $"Se ha actualizado la contraseña de usuario.",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryUser historyUser = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(user),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            UserModifiedId = user.Id,
                            UserId = user.Id
                        };

                        bool success = userDao.Update(user, history, historyUser);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Contraseña actualizada con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar la contraseña. Por favor intentelo nuevamente.";
                            response.httpCode = (int)httpStatusCode;
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
