using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.ConstantsRoles
{
    // Nombres de rol tal como viajan en el claim del JWT, para usar en [Authorize(Roles = ...)].
    // Salen del enum tipoRol para que nunca queden desincronizados.
    public static class RoleNames
    {
        public const string Admin = nameof(tipoRol.Admin);
        public const string Ranger = nameof(tipoRol.Ranger);
        public const string Helper = nameof(tipoRol.Helper);

        // Personal que trabaja en el terreno (reporta incidentes y recibe tareas)
        public const string FieldStaff = Ranger + "," + Helper;

        // Todo el personal interno
        public const string AllStaff = Admin + "," + Ranger + "," + Helper;
    }
}
