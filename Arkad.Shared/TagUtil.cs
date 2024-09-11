namespace Arkad.Shared
{
    public class TagUtil
    {
        #region JWT CLAIMS
        public static string TAG_JWT_CLAIM_JWT_ID = "jwtid";
        public static string TAG_JWT_CLAIM_SESSION_ID = "ssid";
        public static string TAG_JWT_CLAIM_USUARIO_ID = "usuarioId";
        public static string TAG_JWT_PREFIX_AUTHORIZATION_BEARER = "Bearer ";
        public static string TAG_JWT_PREFIX_AUTHORIZATION_BEARER_HTMLENCODE = "Bearer%20";
        #endregion JWT CLAIMS

        #region HEADERS
        public static string TAG_REQUEST_HEADER_AUTHORIZATION = "Authorization";
        public static string TAG_REQUEST_HEADER_CLIENTID = "clientId";
        public static string TAG_REQUEST_HEADER_CLIENTSECRET = "clientSecret";
        public static string TAG_REQUEST_HEADER_CLIENTIP = "client-ip";
        public static string TAG_REQUEST_HEADER_USERAGENT = "User-Agent";
        #endregion HEADERS

        #region CONFIGURACION
        public static string TAG_CONFIG_SECRETKEY = "secretkey";
        public static string TAG_CONFIG_AJUSTES = "AJUSTES";
        public static string TAG_CONFIG_MENSAJE_WEB_INICIAL = "MENSAJEWEBINICIAL";
        public static string TAG_CONFIG_PROYECTO_INFO = "PROYECTOINFO";
        #endregion CONFIGURACION

        #region COOKIES
        public static string TAG_COOKIES_USERNAME = "Username";
        public static string TAG_COOKIES_EMAIL = "Email";
        public static string TAG_COOKIES_MENU_SIDE = "SideMenu";
        public static string TAG_COOKIES_PROYECTO_INFO = "ProyectoInfo";
        public static string TAG_COOKIES_PROYECTO_INFO_THEME = "Theme";
        public static string TAG_COOKIES_PROYECTO_INFO_LOGO = "AppLogo";
        public static string TAG_COOKIES_PROYECTO_INFO_KEYWORDS = "Keywords";
        public static string TAG_COOKIES_PROYECTO_INFO_APPNAME = "AppName";
        public static string TAG_COOKIES_PROYECTO_INFO_ICONO = "AppIcon";
        public static string TAG_COOKIES_PROYECTO_INFO_VERSION = "AppVersion";
        public static string TAG_COOKIES_PROYECTO_INFO_ANIO = "AppAnio";
        #endregion COOKIES

        #region ROLES
        public static string TAG_ROL_SYSADMIN = "SYSADMIN";
        public static string TAG_ROL_ADMIN = "ADMIN";
        public static string TAG_ROL_USER = "USER";
        #endregion ROLES

        #region ASIGNACION
        public static string TAG_ASIGNACION_ITEM_MANDANTE = "MANDANTE";
        public static string TAG_ASIGNACION_ITEM_CONTRATISTA = "CONTRATISTA";
        #endregion ASIGNACION

        #region TIPO REPORTE
        public static string TAG_TIPO_REPORTE_MOVIL = "MOVIL";
        public static string TAG_TIPO_REPORTE_ANIO_ACTUAL = "ANIO_ACTUAL";
        public static string TAG_TIPO_REPORTE_MENSUAL = "MENSUAL";
        #endregion TIPO REPORTE

        #region ESTADO
        public static string TAG_ESTADO_PENDIENTE = "PENDIENTE";
        public static string TAG_ESTADO_PROCESO = "PROCESO";
        public static string TAG_ESTADO_RECHAZADO = "RECHAZADO";
        public static string TAG_ESTADO_APROBADO = "APROBADO";
        public static string TAG_ESTADO_FINALIZADO = "FINALIZADO";
        public static string TAG_ESTADO_ELIMINADO = "ELIMINADO";
        #endregion ESTADO

        #region META_EMPRESA
        public static string TAG_META_EMPRESA_LOGO = "LOGO";
        #endregion META_EMPRESA

        #region HISTORIAL
        public static string TAG_HISTORIAL_CREAR = "CREAR";
        public static string TAG_HISTORIAL_MODIFICAR = "MODIFICAR";
        public static string TAG_HISTORIAL_LEER = "LEER";
        public static string TAG_HISTORIAL_ELIMINAR = "ELIMINAR";
        public static string TAG_HISTORIAL_ACTIVAR = "ACTIVAR";
        public static string TAG_HISTORIAL_DESACTIVAR = "DESACTIVAR";
        #endregion HISTORIAL

        #region TIPO DATO
        public static string TAG_TIPO_DATO_TEXT = "TEXT";
        public static string TAG_TIPO_DATO_TEXT_MULTI = "TEXT_MULTI";
        public static string TAG_TIPO_DATO_NUMERO = "NUMERO";
        #endregion TIPO DATO

        #region Plataforma
        public static string TAG_PLATAFORMA_WEB = "WEB";
        public static string TAG_PLATAFORMA_ANDROID = "ANDROID";
        public static string TAG_PLATAFORMA_IOS = "IOS";
        public static string TAG_PLATAFORMA_WINUI = "WINUI";
        #endregion Plataforma

        #region PRIVILEGIOS
        public static string TAG_PRIVILEGIO_CREAR = "CREAR";
        public static string TAG_PRIVILEGIO_MODIFICAR = "MODIFICAR";
        public static string TAG_PRIVILEGIO_MOSTRAR = "MOSTRAR";
        public static string TAG_PRIVILEGIO_ELIMINAR = "ELIMINAR";
        #endregion PRIVILEGIOS

        #region SOLICITUDES HTTP
        public static string TAG_TIPO_SOLICITUD_HTTP_GET = "GET";
        public static string TAG_TIPO_SOLICITUD_HTTP_POST = "POST";
        public static string TAG_TIPO_SOLICITUD_HTTP_PUT = "PUT";
        public static string TAG_TIPO_SOLICITUD_HTTP_DELETE = "DELETE";
        public static string TAG_TIPO_SOLICITUD_HTTP_PATCH = "PATCH";
        #endregion SOLICITUDES HTTP

        #region NOTIFICACIÓN
        public static string TAG_NOTIFICACION_TIPO_EMAIL = "EMAIL";
        public static string TAG_NOTIFICACION_TIPO_WEB = "WEB";
        public static string TAG_NOTIFICACION_TIPO_MOBILE = "MOBILE";
        #endregion NOTIFICACIÓN
    }
}
