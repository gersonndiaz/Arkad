using Arkad.Server.DAO.Impl;
using Arkad.Server.Helpers;
using Arkad.Server.Models;
using Arkad.Shared;
using Arkad.Shared.Utils.Formula;
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
    public class ItemController : ControllerBase
    {
        private string TAG = typeof(ItemController).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration configuration;

        private string Authorization = "";
        private string clientId;
        private string clientSecret;

        public ItemController(IConfiguration configuration)
        {
            this.configuration = configuration;
            clientSecret = configuration.GetSection("AppSettings").GetSection("ClientAuth").GetSection("Secret").Value;
        }

        #region Items
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

                    string status = Request.Query["status"];

                    bool? bEstado = true;
                    if (!String.IsNullOrEmpty(status))
                    {
                        bEstado = (status.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }

                    #region DAO
                    ItemDao itemDao = new ItemDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    List<Item> items = itemDao.GetAll(bEstado);
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
                        var vItems = (items is not null)
                                                ? items.Adapt<List<Shared.Models.Item>>()
                                                : null;

                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseSuccess;
                        response.subject = "Éxito";
                        response.message = "Información cargada con éxito!";
                        response.httpCode = (int)httpStatusCode;
                        response.data = vItems;
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

                string name = Request.Form["name"];
                string description = Request.Form["description"];
                string type = Request.Form["type"];
                string formula = Request.Form["formula"];
                string formulaAux = Request.Form["formulaAux"];
                string sAuto = Request.Form["auto"];
                string sMonthly = Request.Form["monthly"];
                string groupId = Request.Form["groupId"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(name))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error nombre";
                    response.message = "El nombre del ítem es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(type))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error tipo";
                    response.message = "El tipo del ítem es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    name = name.Trim().ToUpper();
                    description = description.Trim().ToUpper();

                    bool auto = false;
                    if (!String.IsNullOrEmpty(sAuto))
                    {
                        auto = (sAuto.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }

                    bool monthly = false;
                    if (!String.IsNullOrEmpty(sMonthly))
                    {
                        monthly = (sMonthly.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }

                    #region DAO
                    ItemDao itemDao = new ItemDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var itemN = itemDao.GetByName(name);
                    var group = (!String.IsNullOrEmpty(groupId) && DataTypeValidation.checkGuid(groupId))
                                    ? itemDao.GetGroupById(groupId) : null;
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    #endregion DTO

                    bool typeIsValid = false;
                    string formulaIsValid = null;
                    try
                    {
                        if (type.Equals("VALUE", StringComparison.InvariantCultureIgnoreCase))
                        {
                            typeIsValid = true;
                        }
                        else if (type.Equals("AVERAGE", StringComparison.InvariantCultureIgnoreCase))
                        {
                            typeIsValid = true;
                        }
                        else if (type.Equals("FORMULA", StringComparison.InvariantCultureIgnoreCase))
                        {
                            typeIsValid = true;

                            try
                            {
                                formulaIsValid = FormulaUtil.ValidateAndReplaceFormula(formula);
                                Console.WriteLine($"Fórmula validada y reemplazada: {formulaIsValid}");
                            }
                            catch (Exception ex)
                            {
                                formulaIsValid = $"[ERROR]: {ex.Message}";
                                Console.WriteLine($"Error en la validación de la fórmula: {ex.Message}");
                            }
                        }
                        else
                        {
                            typeIsValid = false;
                        }
                    }
                    catch
                    {
                        typeIsValid = false;
                    }

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
                    else if (itemN is not null)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = (itemN.Active)
                                                ? $"El ítem de nombre {name} ya existe!"
                                                : $"El ítem de nombre {name} ya existe y puede activarlo para su uso!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (!typeIsValid)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El tipo [{type}] no es válido!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (typeIsValid
                        && (formulaIsValid is not null && !formulaIsValid.StartsWith("[OK]")))
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"Error en la fórmula: {formulaAux}\n\n" +
                                           $"{formulaIsValid}";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        var items = itemDao.GetAll(true);
                        int position = 1;
                        if (items is not null && items.Count > 0)
                        {
                            foreach(var mItem in items)
                            {
                                mItem.Position = position;
                                position++;
                            }
                        }

                        Item item = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = name,
                            Description = description,
                            Auto = (type.Equals("VALUE", StringComparison.InvariantCultureIgnoreCase)) ? false : true,
                            Position = position,
                            Type = type,
                            Formula = formula,
                            FormulaAux = formulaAux,
                            Monthly = monthly,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            GroupId = (group is not null) ? group.Id : null
                        };

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_CREAR}]",
                            Description = $"Se ha registrado un nuevo ítem:\n\n" +
                                          $"* Nombre: {item.Name}\n" +
                                          $"* Descripción: {item.Description}\n" +
                                          $"* Tipo: {item.Type}\n" +
                                          $"* Fórmula: {item.Formula}\n" +
                                          $"* Fórmula Visible: {item.FormulaAux}\n" +
                                          $"* Posición: {item.Position}\n" +
                                          $"* Auto: {item.Auto}\n" +
                                          $"* Mensual: {item.Monthly}\n" +
                                          $"* Grupo: {((group is not null) ? group.Name : "")}\n",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryItem historyItem = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(item),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            ItemId = item.Id,
                            UserId = user.Id
                        };

                        bool success = itemDao.Save(item, history, historyItem);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Ítem registrado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible registrar el ítem. Por favor intentelo nuevamente.";
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
                string name = Request.Form["name"];
                string description = Request.Form["description"];
                string type = Request.Form["type"];
                string formula = Request.Form["formula"];
                string formulaAux = Request.Form["formulaAux"];
                string sAuto = Request.Form["auto"];
                string sMonthly = Request.Form["monthly"];
                string groupId = Request.Form["groupId"];

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
                    response.message = "El ID del ítem es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkGuid(id))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del ítem seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(name))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error nombre";
                    response.message = "El nombre del ítem es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(type))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error tipo";
                    response.message = "El tipo del ítem es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    name = name.Trim().ToUpper();
                    description = description.Trim().ToUpper();

                    bool auto = false;
                    if (!String.IsNullOrEmpty(sAuto))
                    {
                        auto = (sAuto.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }

                    bool monthly = false;
                    if (!String.IsNullOrEmpty(sMonthly))
                    {
                        monthly = (sMonthly.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }

                    #region DAO
                    ItemDao itemDao = new ItemDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var item = itemDao.GetById(id);
                    var itemN = itemDao.GetByName(name);
                    var group = (!String.IsNullOrEmpty(groupId) && DataTypeValidation.checkGuid(groupId))
                                    ? itemDao.GetGroupById(groupId) : null;
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    #endregion DTO

                    bool typeIsValid = false;
                    string formulaIsValid = null;
                    try
                    {
                        if (type.Equals("VALUE", StringComparison.InvariantCultureIgnoreCase))
                        {
                            typeIsValid = true;
                        }
                        else if (type.Equals("AVERAGE", StringComparison.InvariantCultureIgnoreCase))
                        {
                            typeIsValid = true;
                        }
                        else if (type.Equals("FORMULA", StringComparison.InvariantCultureIgnoreCase))
                        {
                            typeIsValid = true;

                            try
                            {
                                formulaIsValid = FormulaUtil.ValidateAndReplaceFormula(formula);
                                Console.WriteLine($"Fórmula validada y reemplazada: {formulaIsValid}");
                            }
                            catch (Exception ex)
                            {
                                formulaIsValid = $"[ERROR]: {ex.Message}";
                                Console.WriteLine($"Error en la validación de la fórmula: {ex.Message}");
                            }
                        }
                        else
                        {
                            typeIsValid = false;
                        }
                    }
                    catch
                    {
                        typeIsValid = false;
                    }

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
                    else if (item is null)
                    {
                        httpStatusCode = HttpStatusCode.Unauthorized;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error ítem";
                        response.message = "El ítem seleccionado no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (itemN is not null && item.Id != itemN.Id)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = (itemN.Active)
                                                ? $"El ítem de nombre {name} ya existe!"
                                                : $"El ítem de nombre {name} ya existe y puede activarlo para su uso!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (!typeIsValid)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El tipo [{type}] no es válido!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (typeIsValid
                        && (formulaIsValid is not null && !formulaIsValid.StartsWith("[OK]")))
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"Error en la fórmula: {formulaAux}\n\n" +
                                           $"{formulaIsValid}";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        var jItemCurrent = JsonConvert.SerializeObject(item);
                        var itemCurrent = JsonConvert.DeserializeObject<Item>(jItemCurrent);

                        item.Name = name;
                        item.Description = description;
                        item.Type = type;
                        item.Auto = (type.Equals("VALUE", StringComparison.InvariantCultureIgnoreCase)) ? false : true;
                        item.Formula = formula;
                        item.FormulaAux = formulaAux;
                        item.Monthly = monthly;
                        item.ModifiedDate = DateTime.Now;
                        item.GroupId = (group is not null) ? group.Id : null;

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}]",
                            Description = $"Se ha actualizado el ítem:\n\n" +
                                          $"* Nombre: {itemCurrent.Name} => {item.Name}\n" +
                                          $"* Descripción: {itemCurrent.Description} => {item.Description}\n" +
                                          $"* Tipo: {itemCurrent.Type} => {item.Type}\n" +
                                          $"* Fórmula: {itemCurrent.Formula} => {item.Formula}\n" +
                                          $"* Fórmula Visible: {itemCurrent.FormulaAux} => {item.FormulaAux}\n" +
                                          $"* Posición: {itemCurrent.Position} => {item.Position}\n" +
                                          $"* Auto: {itemCurrent.Auto} => {item.Auto}\n" +
                                          $"* Mensual: {itemCurrent.Monthly} => {item.Monthly}\n" +
                                          $"* Grupo: {((itemCurrent.Group is not null) ? itemCurrent.Group.Name : "")} => {((group is not null) ? group.Name : "")}",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryItem historyItem = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(item),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            ItemId = item.Id,
                            UserId = user.Id
                        };

                        bool success = itemDao.Update(item, history, historyItem);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Ítem actualizado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar el ítem. Por favor intentelo nuevamente.";
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
                    response.message = "El ID del ítem es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkGuid(id))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error ID";
                    response.message = "El ID del ítem seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region DAO
                    ItemDao itemDao = new ItemDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var item = itemDao.GetById(id);
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
                    else if (item is null)
                    {
                        httpStatusCode = HttpStatusCode.NotFound;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El ítem seleccionado no ha sido encontrado!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        bool estadoActual = item.Active;

                        item.Active = (item.Active) ? false : true;
                        item.ModifiedDate = DateTime.Now;

                        History history = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = (item.Active)
                                                        ? $"[{TagUtil.TAG_HISTORIAL_ACTIVAR}]"
                                                        : $"[{TagUtil.TAG_HISTORIAL_DESACTIVAR}]",
                            Description = $"Se ha modificado el ítem:\n\n" +
                                          $"* Estado: {estadoActual} => {item.Active}",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryItem historyItem = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(item),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = history.Id,
                            ItemId = item.Id,
                            UserId = user.Id
                        };

                        bool success = itemDao.Update(item, history, historyItem);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Ítem actualizado con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar el ítem. Por favor intentelo nuevamente.";
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

        [HttpPost("v1/update/sort")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> UpdateSort()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string sItems = Request.Form["items"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (String.IsNullOrEmpty(sItems))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error Ítems";
                    response.message = "Debe indicar los ítems que desea ordenar!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region DAO
                    ItemDao itemDao = new ItemDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var items = JsonConvert.DeserializeObject<List<Item>>(sItems);
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
                    else if (items is null)
                    {
                        httpStatusCode = HttpStatusCode.NotFound;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"Los ítems seleccionados no son válidos!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        int posicion = 1;
                        List<Item> mItems = new List<Item>();
                        List<History> histories = new List<History>();
                        List<HistoryItem> historyItems = new List<HistoryItem>();
                        foreach (var item in items)
                        {
                            var mItem = itemDao.GetById(item.Id);
                            mItem.Position = posicion;

                            History history = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}]",
                                Description = $"Se ha modificado el ítem:\n\n" +
                                              $"* Posición: {item.Position} => {mItem.Position}",
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                Active = true
                            };

                            HistoryItem historyItem = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Data = JsonConvert.SerializeObject(item),
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                Active = true,
                                HistoryId = history.Id,
                                ItemId = item.Id,
                                UserId = user.Id
                            };

                            mItems.Add(mItem);
                            histories.Add(history);
                            historyItems.Add(historyItem);

                            posicion++;
                        }

                        bool success = itemDao.Update(mItems, histories, historyItems);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Ítems actualizados con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible actualizar los ítems. Por favor intentelo nuevamente.";
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
        #endregion Items

        #region Histories
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
                string itemId = Request.Query["itemId"];
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
                else if (String.IsNullOrEmpty(itemId))
                {
                    httpStatusCode = HttpStatusCode.NotFound;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error";
                    response.message = "El ítem es obligatorio!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (!DataTypeValidation.checkGuid(itemId))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error";
                    response.message = "El ítem seleccionado no es válido!";
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
                    ItemDao itemDao = new ItemDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var item = itemDao.GetById(itemId);
                    User user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    var histories = itemDao.FindHistories(limit, offset, item, search, true);
                    countTotal = itemDao.CountHistories(item, search, true);
                    var totalPages = Hefesto.Helpers.Utils.calculateTotalPages(countTotal, limit);
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
                    else
                    {
                        List<Shared.Models.HistoryItem> vHistories = new List<Shared.Models.HistoryItem>();

                        if (histories is not null && histories.Count() > 0)
                        {
                            foreach (var h in histories)
                            {
                                var hr = new Shared.Models.HistoryItem();
                                hr.Id = h.Id;
                                hr.CreatedDate = h.CreatedDate;
                                hr.UserName = user.Name;
                                hr.History = h.History.Adapt<Shared.Models.History>();

                                vHistories.Add(hr);
                            }
                        }

                        object data = new
                        {
                            item = item.Name,
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
        #endregion Histories
    }
}
