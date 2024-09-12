using Arkad.Server.DAO;
using Arkad.Server.DAO.Impl;
using Arkad.Server.Helpers;
using Arkad.Server.Models;
using Arkad.Shared;
using Hefesto.Response;
using Hefesto.Validation;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Net;
using System.Web;

namespace Arkad.Server.Areas.Management.Controllers
{
    [Area("management")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class PeriodController : ControllerBase
    {
        private string TAG = typeof(PeriodController).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration configuration;

        private string Authorization = "";
        private string clientId;
        private string clientSecret;

        public PeriodController(IConfiguration configuration)
        {
            this.configuration = configuration;
            clientSecret = configuration.GetSection("AppSettings").GetSection("ClientAuth").GetSection("Secret").Value;
        }

        #region Periods
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

                int? page = (Request.Query["page"].FirstOrDefault() != null && DataTypeValidation.checkNumber(Request.Query["page"].ToString())) ? Int32.Parse(Request.Query["page"].ToString()) : 1;
                string search = Request.Query["search"];

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
                    int limit = 12;
                    int offset = 0;
                    int countTotal = 0;

                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region Definir Limit, Offset y Page
                    if (page != null && page > 0)
                    {
                        if (page == 1)
                        {
                            offset = 0;
                        }
                        else if (page == 2)
                        {
                            offset = limit;
                        }
                        else
                        {
                            offset = ((int)page - 1) * limit;
                        }
                    }
                    else
                    {
                        page = 1;
                        offset = 0;
                    }
                    #endregion Definir Limit, Offset y Page

                    search = (!String.IsNullOrEmpty(search)) ? search.ToUpper() : search;

                    string status = Request.Query["status"];

                    bool? bStatus = true;
                    if (!String.IsNullOrEmpty(status))
                    {
                        bStatus = (status.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }

                    #region DAO
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    List<Period> periods = periodDao.GetAll(limit, offset, search, bStatus);
                    countTotal = periodDao.Count(search, bStatus);
                    var totalPages = Hefesto.Helpers.Utils.calculateTotalPages(countTotal, limit);
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
                    else if (role.Code != "SYSADMIN" && role.Code != "ADMIN")
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error usuario";
                        response.message = "Su usuario no está autorizado para acceder a esta información!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    #endregion VALIDACION
                    else
                    {
                        var vPeriods = (periods is not null)
                                                ? periods.Adapt<List<Shared.Models.Period>>()
                                                : null;

                        object data = new
                        {
                            periods = vPeriods,
                            totalData = countTotal,
                            totalPages = totalPages,
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

        [HttpPost("v1/create")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> Create()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string sYear = Request.Form["year"];
                string sMonth = Request.Form["month"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(sYear))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error año";
                    response.message = "El año del periodo es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkNumber(sYear))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error año";
                    response.message = "El año del periodo no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(sMonth))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error mes";
                    response.message = "El mes del periodo es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkNumber(sMonth))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error mes";
                    response.message = "El mes del periodo no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    int year = Int32.Parse(sYear);
                    int month = Int32.Parse(sMonth);

                    #region DAO
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var periodAux = periodDao.GetByYearMonth(year, month);
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    #endregion DTO

                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Acceso no autorizado!";
                        response.message = $"Su usuario no se encuentra autorizado!";
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
                    else if (role.Code != "SYSADMIN" && role.Code != "ADMIN")
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error usuario";
                        response.message = "Su usuario no está autorizado para acceder a esta información!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (periodAux is not null)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = (periodAux.Active)
                                                ? $"El periodo {year}/{month} ya existe!"
                                                : $"El periodo  {year}/{month} ya existe y puede activarlo para su uso!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (year < 2000 || year > DateTime.Now.Year)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error periodo";
                        response.message = $"El año [{year}] no puede ser menor a [2000] ni mayor al año en curso [{DateTime.Now.Year}]!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (month < 1 || month > 12)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error periodo";
                        response.message = $"El mes [{month}] no es válido (meses válidos del 1 al 12)!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        var periodCurent = periodDao.GetCurrent();

                        Period period = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Year = year,
                            Month = month,
                            Current = (periodCurent is null) ? true : false,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_CREAR}]",
                            Description = $"Se ha registrado un nuevo periodo:\n\n" +
                                          $"* Año: {period.Year}\n" +
                                          $"* Mes: {period.Month}\n",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryPeriod historyPeriod = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(period),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            PeriodId = period.Id,
                            UserId = user.Id
                        };

                        bool success = periodDao.Save(period, history, historyPeriod);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Periodo registrado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible registrar el periodo. Por favor intentelo nuevamente.";
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

                string id = Request.Form["id"];
                string sYear = Request.Form["year"];
                string sMonth = Request.Form["month"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(id))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del periodo es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkGuid(id))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del periodo seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(sYear))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error año";
                    response.message = "El año del periodo es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkNumber(sYear))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error año";
                    response.message = "El año del periodo no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(sMonth))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error mes";
                    response.message = "El mes del periodo es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkNumber(sMonth))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error mes";
                    response.message = "El mes del periodo no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    int year = Int32.Parse(sYear);
                    int month = Int32.Parse(sMonth);

                    #region DAO
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var period = periodDao.GetById(id);
                    var periodAux = periodDao.GetByYearMonth(year, month);
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    #endregion DTO

                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Acceso no autorizado!";
                        response.message = $"Su usuario no se encuentra autorizado!";
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
                    else if (role.Code != "SYSADMIN" && role.Code != "ADMIN")
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error usuario";
                        response.message = "Su usuario no está autorizado para acceder a esta información!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (period is null)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error periodo";
                        response.message = "El periodo seleccionado no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (periodAux is not null && period.Id != periodAux.Id)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = (periodAux.Active)
                                                ? $"El periodo {year}/{month.ToString("D2")} ya existe!"
                                                : $"El periodo  {year}/{month.ToString("D2")} ya existe y puede activarlo para su uso!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (year < 2000 || year > DateTime.Now.Year)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error periodo";
                        response.message = $"El año [{year}] no puede ser menor a [2000] ni mayor al año en curso [{DateTime.Now.Year}]!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (month < 1 || month > 12)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error periodo";
                        response.message = $"El mes [{month}] no es válido (meses válidos del 1 al 12)!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        var jPeriodCurrent = JsonConvert.SerializeObject(period);
                        var periodCurrent = JsonConvert.DeserializeObject<Period>(jPeriodCurrent);

                        period.Year = year;
                        period.Month = month;

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}]",
                            Description = $"Se ha actualizado el periodo:\n\n" +
                                          $"* Año: {periodCurrent.Year} => {period.Year}\n" +
                                          $"* Mes: {periodCurrent.Month} => {periodCurrent.Month}\n",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryPeriod historyPeriod = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(period),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            PeriodId = period.Id,
                            UserId = user.Id
                        };

                        bool success = periodDao.Update(period, history, historyPeriod);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Periodo actualizado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar el periodo. Por favor intentelo nuevamente.";
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

        [HttpPost("v1/update/status")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> UpdateStatus()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string id = Request.Form["id"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(id))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del periodo es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkGuid(id))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del periodo seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region DAO
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var period = periodDao.GetById(id);
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    #endregion DTO

                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Acceso no autorizado!";
                        response.message = $"Su usuario no se encuentra autorizado!";
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
                    else if (role.Code != "SYSADMIN" && role.Code != "ADMIN")
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error usuario";
                        response.message = "Su usuario no está autorizado para acceder a esta información!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (period is null)
                    {
                        httpStatusCode = HttpStatusCode.NotFound;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El periodo seleccionado no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        bool currentStatus = period.Active;

                        period.Active = (period.Active) ? false : true;

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = (period.Active)
                                                        ? $"[{TagUtil.TAG_HISTORIAL_ACTIVAR}]"
                                                        : $"[{TagUtil.TAG_HISTORIAL_DESACTIVAR}]",
                            Description = $"Se ha modificado el periodo:\n\n" +
                                          $"* Estado: {currentStatus} => {period.Active}",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryPeriod historyPeriod = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(period),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            PeriodId = period.Id,
                            UserId = user.Id
                        };

                        bool success = periodDao.Update(period, history, historyPeriod);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Periodo actualizado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar el periodo. Por favor intentelo nuevamente.";
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

        [HttpPost("v1/update/current")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> UpdateCurrent()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string id = Request.Form["id"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(id))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del periodo es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkGuid(id))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del periodo seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region DAO
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var period = periodDao.GetById(id);
                    var periodCurrent = periodDao.GetCurrent();
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    #endregion DTO

                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Acceso no autorizado!";
                        response.message = $"Su usuario no se encuentra autorizado!";
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
                    else if (role.Code != "SYSADMIN" && role.Code != "ADMIN")
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error usuario";
                        response.message = "Su usuario no está autorizado para acceder a esta información!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (period is null)
                    {
                        httpStatusCode = HttpStatusCode.NotFound;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El periodo seleccionado no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        bool eCurrent = true;
                        History historyPrev = null;
                        HistoryPeriod historyPeriodPrev = null;

                        if (periodCurrent is not null && (period.Id == periodCurrent.Id))
                        {
                            // El periodo activo es el mismo que se esta modificando
                        }
                        else if (periodCurrent is not null)
                        {
                            eCurrent = periodCurrent.Current;
                            periodCurrent.Current = false;

                            historyPrev = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}]",
                                Description = $"Se ha cerrado el periodo:\n\n" +
                                              $"* Estado: {eCurrent} => {periodCurrent.Active}.\n\n" +
                                              $"Se ha abierto el periodo [{period.Year}/{period.Month}]",
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                Active = true
                            };

                            historyPeriodPrev = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Data = JsonConvert.SerializeObject(period),
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                Active = true,
                                HistoryId = historyPrev.Id,
                                PeriodId = period.Id,
                                UserId = user.Id
                            };
                        }

                        bool currentStatus = period.Current;
                        period.Current = (period.Current) ? false : true;

                        // Como se va a cerrar el periodo, no tiene sentido cerrar otro
                        if (period.Current == false)
                        {
                            periodCurrent = null;
                        }

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}]",
                            Description = $"Se ha {((period.Current) ? "abierto" : "cerrado")} el periodo:\n\n" +
                                          $"* Estado: {currentStatus} => {period.Active}.",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        if (historyPeriodPrev is not null)
                        {
                            history.Description += $"\n\nSe ha cerrado el periodo [{periodCurrent.Year}/{periodCurrent.Month}].";
                        }

                        HistoryPeriod historyPeriod = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(period),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            PeriodId = period.Id,
                            UserId = user.Id
                        };

                        bool success = (historyPeriodPrev is null)
                                            ? periodDao.Update(period, history, historyPeriod)
                                            : periodDao.Update(period, history, historyPeriod, periodCurrent, historyPrev, historyPeriodPrev);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Periodo actualizado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar el periodo. Por favor intentelo nuevamente.";
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
        #endregion Periods

        #region Historial
        [HttpGet("v1/history")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> FindHistory()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                #region Params
                string periodId = Request.Query["periodId"];
                int? page = (Request.Query["page"].FirstOrDefault() != null && DataTypeValidation.checkNumber(Request.Query["page"].ToString())) ? Int32.Parse(Request.Query["page"].ToString()) : 1;
                string search = Request.Query["search"];
                #endregion Params

                #region VALIDACION
                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado";
                    response.message = "Acceso no autorizado!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(periodId))
                {
                    httpStatusCode = HttpStatusCode.NotFound;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error";
                    response.message = "El period es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkGuid(periodId))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error";
                    response.message = "El period seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                #endregion VALIDACION
                else
                {
                    int limit = 10;
                    int offset = 0;
                    int countTotal = 0;

                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region Definir Limit, Offset y Page
                    if (page != null && page > 0)
                    {
                        if (page == 1)
                        {
                            offset = 0;
                        }
                        else if (page == 2)
                        {
                            offset = limit;
                        }
                        else
                        {
                            offset = ((int)page - 1) * limit;
                        }
                    }
                    else
                    {
                        page = 1;
                        offset = 0;
                    }
                    #endregion Definir Limit, Offset y Page

                    search = (!String.IsNullOrEmpty(search)) ? search.ToUpper() : search;

                    #region DAO
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var period = periodDao.GetById(periodId);
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    var histories = periodDao.FindHistory(limit, offset, period, search, true);
                    countTotal = periodDao.CountHistory(period, search, true);
                    var totalPages = Hefesto.Helpers.Utils.calculateTotalPages(countTotal, limit);
                    #endregion DTO

                    if (user is null || !user.Active)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Acceso no autorizado!";
                        response.message = $"Su user no se encuentra autorizado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (role is null)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error rol";
                        response.message = "El rol de user no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (role.Code != "SYSADMIN" && role.Code != "ADMIN")
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error user";
                        response.message = "Su user no está autorizado para acceder a esta información!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        var vHistories = new List<Shared.Models.HistoryPeriod>();

                        if (histories is not null && histories.Count() > 0)
                        {
                            foreach (var history in histories)
                            {
                                var hr = new Shared.Models.HistoryPeriod();
                                hr.Id = history.Id;
                                hr.CreatedDate = history.CreatedDate;
                                hr.UserName = user.Name;
                                hr.History = history.History.Adapt<Shared.Models.History>();

                                vHistories.Add(hr);
                            }
                        }

                        object data = new
                        {
                            period = $"{period.Year}/{period.Month.ToString("D2")}",
                            histories = vHistories,
                            totalData = countTotal,
                            totalPages = totalPages,
                        };

                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseSuccess;
                        response.subject = "Éxito";
                        response.message = "Historial cargado con éxito!";
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
        #endregion Historial
    }
}
