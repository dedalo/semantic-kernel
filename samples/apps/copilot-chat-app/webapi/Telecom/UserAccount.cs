namespace SemanticKernel.Service.Telecom
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string cuic { get; set; }
        public string Phone { get; set; }
        public string Extention { get; set; }
        public string WorkPlace { get; set; }
        public string Status { get; set; }
        public int Audit { get; set; }
        public string Code { get; set; }
        public string WorkShift { get; set; }
        public string Workstation { get; set; }
        public string RAZONSOCIAL { get; set; }
        public string NUMERO_IDENTIFICACION_CLIENTE { get; set; }
        public int ID_CLASIFICACION_CLIENTE2 { get; set; }
        public string CLASIFICACION_CLIENTE2 { get; set; }
        public string AccessToken { get; set; }
        public int Administrator { get; set; }
        public string ID_CLIENTE_TF { get; set; }
        public string ID_CLIENTE_OPEN { get; set; }
        public string ID_CLIENTE_PM { get; set; }
    }

    public class UserAccountResponse
    {
        public int total { get; set; }
        public UserAccount[] rows { get; set; }
        public bool success { get; set; }
    }
}
