namespace Application.Features.Auth.Commands.ForgotPassword;

public static class ForgotPasswordMessages
{
    /// <summary>
    /// Generic message returned whether or not the email exists (prevents account enumeration).
    /// </summary>
    public const string GenericSuccess =
        "Se este e-mail estiver cadastrado, enviaremos instruções para redefinir a senha em instantes.";
}
