using Arkad.Server.DAO.Impl;
using Arkad.Server.Helpers;
using Arkad.Server.Models;
using Arkad.Shared;
using Arkad.Shared.Utils.Formula;
using Hefesto.Response;
using Hefesto.Validation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Net;
using System.Web;

namespace Arkad.Server.Areas.Expense.Controllers
{
    [Area("expense")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ControlController : ControllerBase
    {
        private string TAG = typeof(ControlController).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration configuration;

        private string Authorization = "";
        private string clientId;
        private string clientSecret;

        public ControlController(IConfiguration configuration)
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
                    ExpenseDao expenseDao = new ExpenseDao();
                    ItemDao itemDao = new ItemDao();
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    Period period = periodDao.GetCurrent();
                    List<Item> items = itemDao.GetAll(true);
                    List<Models.Expense> expenses = (period is not null) 
                                                        ? expenseDao.GetAll(period, true)
                                                        : null;
                    ExpenseControl control = (period is not null)
                                                        ? expenseDao.GetControlByPeriod(period, true)
                                                        : null;
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
                        //var vPeriod = (period is not null) ? period.Adapt<Shared.Models.Period>() : null;
                        //var vItems = (items is not null) ? items.Adapt<List<Shared.Models.Item>>() : null;
                        //var vExpenses = (expenses is not null) ? expenses.Adapt<List<Shared.Models.Expense>>() : null;
                        //var vControl = (control is not null) ? control.Adapt<Shared.Models.ExpenseControl>() : null;
                        //var vRole = (role is not null) ? role.Adapt<Shared.Models.Role>() : null;

                        //object data = new
                        //{
                        //    period = vPeriod,
                        //    items = vItems,
                        //    expenses = vExpenses,
                        //    control = vControl,
                        //    role = vRole
                        //};

                        if (expenses is not null)
                        {
                            foreach(var expense in expenses)
                            {
                                expense.User = null;
                                expense.UserId = null;
                            }
                        }

                        if (control is not null)
                        {
                            control.User = null;
                            control.UserId = null;
                        }

                        object data = new
                        {
                            period = period,
                            items = items,
                            expenses = expenses,
                            control = control,
                            role = role
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

        [HttpPost("v1/save")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> Save()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string expenseControlId = Request.Form["expenseControlId"];
                string sExpenses = Request.Form["expenses"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Acceso no autorizado!";
                    response.message = "Usuario y/o contraseña incorrecta!";
                    response.httpCode = (int)httpStatusCode;
                    response.urlRedirect = $"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}";
                }
                else if (!String.IsNullOrEmpty(expenseControlId)
                        && !DataTypeValidation.checkGuid(expenseControlId))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error control";
                    response.message = "El Control seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(sExpenses))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error gastos";
                    response.message = "Los indicadores de gastos son obligatorios!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region DAO
                    ExpenseDao expenseDao = new ExpenseDao();
                    ItemDao itemDao = new ItemDao();
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    User user = userDao.GetById(userId);
                    var jExpenses = JsonConvert.DeserializeObject<List<Shared.Models.Expense>>(sExpenses);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    var periodCurrent = periodDao.GetCurrent();

                    var expenseControl = (!String.IsNullOrEmpty(expenseControlId)
                                        && DataTypeValidation.checkGuid(expenseControlId))
                                                ? expenseDao.GetControlById(expenseControlId)
                                                : null;
                    var expenseControlPeriod = (periodCurrent is not null)
                                                    ? expenseDao.GetControlByPeriod(periodCurrent, true)
                                                    : null;
                    #endregion DTO

                    bool expenseControlValid = true;
                    if (expenseControl is not null && expenseControlPeriod is not null)
                    {
                        expenseControlValid = (expenseControl.Id == expenseControlPeriod.Id);
                    }
                    else if (expenseControl is not null && expenseControlPeriod is null)
                    {
                        expenseControlValid = false;
                    }
                    else if (expenseControl is null && expenseControlPeriod is not null)
                    {
                        expenseControlValid = false;
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
                    //else if (rol.Codigo != "SYSADMIN" && rol.Codigo != "ADMIN")
                    //{
                    //    httpStatusCode = HttpStatusCode.Unauthorized;
                    //    response.status = StatusResponseCodes.StatusResponseError;
                    //    response.subject = "Error usuario";
                    //    response.message = "Su usuario no está autorizado para acceder a esta información!";
                    //    response.httpCode = (int)httpStatusCode;
                    //}
                    else if (jExpenses is null)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"Los indicadores de gastos no son válidos!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (periodCurrent is null)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El periodo seleccionado no es válido!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (!expenseControlValid)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El control de gastos no es válido!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (expenseControl is null)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El control de gastos no existe!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        List<Shared.Models.Expense> ExpensesAux = JsonConvert.DeserializeObject<List<Shared.Models.Expense>>(expenseControl.Data);

                        if (jExpenses is not null && jExpenses.Count > 0)
                        {
                            foreach(var mExpense in jExpenses)
                            {
                                var vExpense = ExpensesAux.FirstOrDefault(x => x.Id == mExpense.Id);
                                vExpense.Value = mExpense.Value;
                                vExpense.Explanation = mExpense.Explanation;
                                vExpense.ModifiedDate = DateTime.Now;

                                var expense = expenseDao.GetById(vExpense.Id);
                                expense.Value = mExpense.Value;
                                expense.Explanation = mExpense.Explanation;
                                expense.ModifiedDate = DateTime.Now;
                            }
                        }

                        expenseControl.Data = JsonConvert.SerializeObject(ExpensesAux);
                        expenseControl.ModifiedDate = DateTime.Now;

                        History historyControl = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{TagUtil.TAG_HISTORIAL_MODIFICAR}]",
                            Description = $"Se ha registrado nueva información",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryExpenseControl historyExpenseControl = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(expenseControl),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = historyControl.Id,
                            ExpenseControlId = expenseControl.Id,
                            UserId = user.Id
                        };

                        bool success = expenseDao.UpdateControl(expenseControl, historyControl, historyExpenseControl);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Indicadores registrados con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible registrar los indicadores. Por favor intentelo nuevamente.";
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

        [HttpPost("v1/update/items")]
        [Produces("application/json")]
        public async Task<ResponseGenericModel> UpdateItems()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;

            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Headers[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION].ToString();

                string expenseControlId = Request.Form["expenseControlId"];
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
                else if (!String.IsNullOrEmpty(expenseControlId)
                        && !DataTypeValidation.checkGuid(expenseControlId))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error control";
                    response.message = "El Control seleccionado no es válido!";
                    response.httpCode = (int)httpStatusCode;
                }
                else if (String.IsNullOrEmpty(sItems))
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                    response.status = StatusResponseCodes.StatusResponseError;
                    response.subject = "Error items";
                    response.message = "Los items son obligatorios!";
                    response.httpCode = (int)httpStatusCode;
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Authorization, clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    #region DAO
                    ExpenseDao expenseDao = new ExpenseDao();
                    ItemDao itemDao = new ItemDao();
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    User user = userDao.GetById(userId);
                    var jItems = JsonConvert.DeserializeObject<List<Shared.Models.Item>>(sItems);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    var periodCurrent = periodDao.GetCurrent();

                    var expenses = (periodCurrent is not null)
                                        ? expenseDao.GetAll(periodCurrent, true)
                                        : null;

                    var expenseControl = (!String.IsNullOrEmpty(expenseControlId)
                                        && DataTypeValidation.checkGuid(expenseControlId))
                                                ? expenseDao.GetControlById(expenseControlId)
                                                : null;
                    var expenseControlPeriod = (periodCurrent is not null)
                                                    ? expenseDao.GetControlByPeriod(periodCurrent, true)
                                                    : null;
                    #endregion DTO

                    bool expenseControlValid = true;
                    if (expenseControl is not null && expenseControlPeriod is not null)
                    {
                        expenseControlValid = (expenseControl.Id == expenseControlPeriod.Id);
                    }
                    else if (expenseControl is not null && expenseControlPeriod is null)
                    {
                        expenseControlValid = false;
                    }
                    else if (expenseControl is null && expenseControlPeriod is not null)
                    {
                        expenseControlValid = false;
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
                    //else if (rol.Codigo != "SYSADMIN" && rol.Codigo != "ADMIN")
                    //{
                    //    httpStatusCode = HttpStatusCode.Unauthorized;
                    //    response.status = StatusResponseCodes.StatusResponseError;
                    //    response.subject = "Error usuario";
                    //    response.message = "Su usuario no está autorizado para acceder a esta información!";
                    //    response.httpCode = (int)httpStatusCode;
                    //}
                    else if (jItems is null)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"Los items no son válidos!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (periodCurrent is null)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El periodo seleccionado no es válido!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else if (!expenseControlValid)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        response.status = StatusResponseCodes.StatusResponseError;
                        response.subject = "Error";
                        response.message = $"El control de gastos no es válido!";
                        response.httpCode = (int)httpStatusCode;
                    }
                    else
                    {
                        List<Models.Expense> ExpensesNew = new List<Models.Expense>();
                        List<Shared.Models.Expense> ExpensesNewAux = new List<Shared.Models.Expense>();
                        List<FormulaVariable> ItemsFormulaCalc = new List<FormulaVariable>();

                        if (expenses is not null && expenses.Count > 0)
                        {
                            foreach(var expense in expenses)
                            {
                                var existingVariable = ItemsFormulaCalc.FirstOrDefault(x => x.Id == expense.Id);

                                if (existingVariable is null)
                                {
                                    ItemsFormulaCalc.Add(new FormulaVariable { Id = expense.Id, Value = expense.Value });
                                }
                            }
                        }                        

                        #region Items
                        if (jItems is not null)
                        {
                            foreach (var mItem in jItems)
                            {
                                var item = itemDao.GetById(mItem.Id);
                                var expense = expenseDao.GetByPeriodAndItem(periodCurrent, item, true);

                                if (expense is null)
                                {
                                    expense = new Models.Expense();
                                    expense.Id = Guid.NewGuid().ToString();
                                    expense.Value = 0;
                                    expense.Explanation = null;
                                    expense.position = 1;
                                    expense.CreatedDate = DateTime.Now;
                                    expense.ModifiedDate = DateTime.Now;
                                    expense.Active = true;
                                    expense.ItemId = item.Id;
                                    expense.PeriodId = periodCurrent.Id;
                                    expense.UserId = user.Id;


                                    string jItem = JsonConvert.SerializeObject(mItem);
                                    string jPeriod = JsonConvert.SerializeObject(periodCurrent);
                                    string jUser = JsonConvert.SerializeObject(user);

                                    var vItem = JsonConvert.DeserializeObject<Shared.Models.Item>(jItem);
                                    var vPeriod = JsonConvert.DeserializeObject<Shared.Models.Period>(jPeriod);
                                    var vUser = JsonConvert.DeserializeObject<Shared.Models.User>(jUser);
                                    vUser.Password = null;

                                    var mExpense = expense.Adapt<Shared.Models.Expense>();
                                    mExpense.Item = vItem;
                                    mExpense.Period = vPeriod;
                                    mExpense.User = vUser;

                                    ExpensesNew.Add(expense);
                                    ExpensesNewAux.Add(mExpense);
                                }
                                else
                                {
                                    var mExpense = expense.Adapt<Shared.Models.Expense>();
                                    ExpensesNewAux.Add(mExpense);
                                }
                            }
                        }
                        #endregion Items

                        if (expenses is not null && expenses.Count > 0)
                        {
                            foreach (var expense in expenses)
                            {
                                bool exist = jItems.Any(x => x.Id == expense.ItemId);

                                if (!exist)
                                {
                                    expense.ModifiedDate = DateTime.Now;
                                    expense.Active = false;
                                }
                            }
                        }

                        bool newHistory = false;
                        if (expenseControl is null)
                        {
                            expenseControl = new ExpenseControl();
                            expenseControl.Id = Guid.NewGuid().ToString();
                            expenseControl.Data = JsonConvert.SerializeObject(ExpensesNewAux);
                            expenseControl.Explanation = $"Control de Gastos [{periodCurrent.Year}/{periodCurrent.Month.ToString("D2")}]";
                            expenseControl.Finished = false;
                            expenseControl.CreatedDate = DateTime.Now;
                            expenseControl.ModifiedDate = DateTime.Now;
                            expenseControl.Active = true;
                            expenseControl.PeriodId = periodCurrent.Id;
                            expenseControl.UserId = user.Id;

                            newHistory = true;
                        }
                        else
                        {
                            expenseControl.Data = JsonConvert.SerializeObject(ExpensesNewAux);
                            expenseControl.ModifiedDate = DateTime.Now;

                            newHistory = false;
                        }

                        History historyControl = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Explanation = $"[{((newHistory) ? TagUtil.TAG_HISTORIAL_CREAR : TagUtil.TAG_HISTORIAL_MODIFICAR)}]",
                            Description = $"Se ha registrado nueva información",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true
                        };

                        HistoryExpenseControl historyExpenseControl = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Data = JsonConvert.SerializeObject(expenseControl),
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Active = true,
                            HistoryId = historyControl.Id,
                            ExpenseControlId = expenseControl.Id,
                            UserId = user.Id
                        };

                        bool success = expenseDao.UpdateControl(ExpensesNew, expenseControl, historyControl, historyExpenseControl);

                        if (success)
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseSuccess;
                            response.subject = "Éxito";
                            response.message = "Indicadores registrados con éxito!";
                            response.httpCode = (int)httpStatusCode;
                        }
                        else
                        {
                            httpStatusCode = HttpStatusCode.OK;
                            response.status = StatusResponseCodes.StatusResponseError;
                            response.subject = "Error";
                            response.message = "No fue posible registrar los indicadores. Por favor intentelo nuevamente.";
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
