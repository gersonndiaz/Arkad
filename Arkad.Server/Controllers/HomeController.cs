using Arkad.Server.DAO.Impl;
using Arkad.Server.Helpers;
using Arkad.Server.Models;
using Arkad.Shared;
using Arkad.Shared.Utils.Formula;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Hefesto.Response;
using Hefesto.Validation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace Arkad.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private string TAG = typeof(HomeController).FullName;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration configuration;

        private string Authorization = "";
        private string clientId;
        private string clientSecret;

        public HomeController(IConfiguration configuration)
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

                string sMonths = Request.Query["months"];

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

                    int months = 3;

                    if (!String.IsNullOrEmpty(sMonths))
                    {
                        try
                        {
                            if (Hefesto.Validation.DataTypeValidation.checkNumber(sMonths)
                                && (sMonths.Equals("1", StringComparison.InvariantCultureIgnoreCase)
                                || sMonths.Equals("3", StringComparison.InvariantCultureIgnoreCase)
                                || sMonths.Equals("6", StringComparison.InvariantCultureIgnoreCase)
                                || sMonths.Equals("12", StringComparison.InvariantCultureIgnoreCase))
                               )
                            {
                                months = int.Parse(sMonths);
                            }
                        }
                        catch { }
                    }

                    #region DAO
                    ExpenseDao expenseDao = new ExpenseDao();
                    ItemDao itemDao = new ItemDao();
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    var controls = expenseDao.GetControlAllByMonths(months, true);
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
                        dynamic controlData = new ExpandoObject();
                        controlData.Items = new List<dynamic>();

                        if (controls is not null)
                        {
                            foreach (var control in controls)
                            {
                                control.User = null;
                                control.UserId = null;

                                dynamic cData = new ExpandoObject();
                                cData.Year = control.Period.Year;
                                cData.Month = control.Period.Month;
                                cData.Total = 0;

                                List<Shared.Models.Expense> expenses = JsonConvert.DeserializeObject<List<Shared.Models.Expense>>(control.Data);
                                if (expenses is not null && expenses.Count > 0)
                                {
                                    foreach(var expense in expenses)
                                    {
                                        if (!expense.Item.Auto)
                                        {
                                            cData.Total += expense.Value;
                                        }
                                    }
                                }
                                controlData.Items.Add(cData);
                            }
                        }

                        object data = new
                        {
                            controlData = controlData,
                            controls = controls,
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

        [HttpGet("v1/export/excel")]
        public IActionResult ExportToExcel()
        {
            ResponseGenericModel response = new ResponseGenericModel();
            var httpStatusCode = HttpStatusCode.OK;
            try
            {
                #region Referer
                string referer = Request.Headers["Referer"].ToString();
                #endregion Referer

                string Authorization = Request.Query[TagUtil.TAG_REQUEST_HEADER_AUTHORIZATION];

                string sMonths = Request.Query["months"];

                if (String.IsNullOrEmpty(Authorization))
                {
                    return Redirect($"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado!")}&returnUrl={referer}");
                }
                else
                {
                    string userId = SecurityUtils.GetValByTypeFromToken(Hefesto.Encode.EncodeUtil.decodeBase64ToString(Authorization), clientSecret, TagUtil.TAG_JWT_CLAIM_USUARIO_ID);

                    int months = 3;

                    if (!String.IsNullOrEmpty(sMonths))
                    {
                        try
                        {
                            if (Hefesto.Validation.DataTypeValidation.checkNumber(sMonths)
                                && (sMonths.Equals("1", StringComparison.InvariantCultureIgnoreCase)
                                || sMonths.Equals("3", StringComparison.InvariantCultureIgnoreCase)
                                || sMonths.Equals("6", StringComparison.InvariantCultureIgnoreCase)
                                || sMonths.Equals("12", StringComparison.InvariantCultureIgnoreCase))
                               )
                            {
                                months = int.Parse(sMonths);
                            }
                        }
                        catch { }
                    }

                    #region DAO
                    ExpenseDao expenseDao = new ExpenseDao();
                    ItemDao itemDao = new ItemDao();
                    PeriodDao periodDao = new PeriodDao();
                    UserDao userDao = new UserDao();
                    #endregion DAO

                    #region DTO
                    var user = userDao.GetById(userId);
                    var role = (user is not null) ? userDao.GetRoleById(user.RoleId) : null;
                    var controls = expenseDao.GetControlAllByMonths(months, true);
                    #endregion DTO

                    #region VALIDACION
                    if (user is null || !user.Active)
                    {
                        return Redirect($"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado.")}&returnUrl={Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
                    }
                    else if (role is null)
                    {
                        return Redirect($"/error?code=403&message={HttpUtility.UrlEncode("Acceso no autorizado.")}&returnUrl={Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
                    }
                    #endregion VALIDACION
                    else
                    {
                        return ExportDataToExcel(controls);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(TAG + " -- " + e);
                return Redirect($"/error?code=500&message={HttpUtility.UrlEncode("Se produjo un error inesperado.")}&returnUrl={Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
            }
        }

        private FileStreamResult ExportDataToExcel(List<ExpenseControl> controls)
        {
            using (var workbook = new XLWorkbook())
            {
                foreach (var control in controls)
                {
                    // Crear una nueva hoja por cada control
                    string sheetName = $"{control.Period.Year}-{control.Period.Month.ToString("D2")}";
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    // Configurar la cabecera de la hoja
                    worksheet.Cell(1, 1).Value = "Ítem";
                    worksheet.Cell(1, 2).Value = "Monto";
                    worksheet.Cell(1, 3).Value = "Explicación";
                    worksheet.Cell(1, 4).Value = "Fecha";
                    worksheet.Row(1).Style.Font.Bold = true;
                    worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.Aqua;

                    // Deserializar los gastos de control.Data
                    List<Shared.Models.Expense> expenses =
                        JsonConvert.DeserializeObject<List<Shared.Models.Expense>>(control.Data);

                    int currentRow = 2;

                    if (expenses != null && expenses.Count > 0)
                    {
                        foreach (var expense in expenses)
                        {
                            // Rellenar los datos en cada fila
                            worksheet.Cell(currentRow, 1).Value = expense.Item?.Name ?? "Sin nombre";
                            worksheet.Cell(currentRow, 2).Value = expense.Value;
                            worksheet.Cell(currentRow, 3).Value = expense.Explanation ?? "Sin explicación";
                            worksheet.Cell(currentRow, 4).Value = expense.CreatedDate.ToString("yyyy-MM-dd");

                            // **Aplicar formato de moneda** a la celda del monto
                            var montoCell = worksheet.Cell(currentRow, 2);
                            montoCell.Style.NumberFormat.Format = "$#,##0.00"; // Formato de moneda

                            // Aplicar un estilo condicional
                            if (expense.Item.Auto)
                            {
                                var row = worksheet.Row(currentRow);
                                row.Style.Fill.BackgroundColor = XLColor.Gray;
                                row.Style.Font.FontColor = XLColor.White;
                            }

                            currentRow++;
                        }
                    }

                    // Añadir filtros automáticos
                    worksheet.RangeUsed().SetAutoFilter();

                    // Ajustar el ancho de las columnas
                    worksheet.Columns().AdjustToContents();
                }

                // Creamos el Objeto en memoria
                MemoryStream wbStream = new MemoryStream();
                workbook.SaveAs(wbStream);
                workbook.Dispose();

                wbStream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(wbStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_ControlGastos.xlsx"
                };
            }
        }

    }
}
