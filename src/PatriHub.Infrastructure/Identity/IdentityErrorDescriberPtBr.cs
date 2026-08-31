using Microsoft.AspNetCore.Identity;

namespace PatriHub.Infrastructure.Identity;

/// <summary>
/// Traduz pra pt-BR as mensagens padrão do ASP.NET Core Identity — sem isso, `IdentityError
/// .Description` vaza texto em inglês pro usuário final via `AutenticacaoService.RegistrarAsync`
/// (ex.: "Passwords must be at least 8 characters."), destoando do resto do app, que é 100%
/// pt-BR (ver 00-CONSTITUTION.md, "Mercado/idioma/moeda: Brasil, pt-BR, BRL — únicos
/// suportados no MVP"). Registrado via `.AddErrorDescriber&lt;IdentityErrorDescriberPtBr&gt;()`
/// em `DependencyInjection.cs`.
/// </summary>
public sealed class IdentityErrorDescriberPtBr : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new()
    {
        Code = nameof(DefaultError),
        Description = "Ocorreu um erro desconhecido.",
    };

    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = "Falha de concorrência: o registro foi modificado por outra requisição.",
    };

    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = "Senha incorreta.",
    };

    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = "Token inválido.",
    };

    public override IdentityError RecoveryCodeRedemptionFailed() => new()
    {
        Code = nameof(RecoveryCodeRedemptionFailed),
        Description = "Falha ao usar o código de recuperação.",
    };

    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = "Já existe uma conta associada a esse login.",
    };

    public override IdentityError InvalidUserName(string? userName) => new()
    {
        Code = nameof(InvalidUserName),
        Description = $"O nome de usuário '{userName}' é inválido — só pode conter letras ou dígitos.",
    };

    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = $"O email '{email}' é inválido.",
    };

    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = $"O nome de usuário '{userName}' já está em uso.",
    };

    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = $"O email '{email}' já está em uso.",
    };

    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = $"O papel '{role}' é inválido.",
    };

    public override IdentityError DuplicateRoleName(string role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = $"O papel '{role}' já existe.",
    };

    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = "O usuário já tem uma senha definida.",
    };

    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = "O bloqueio de conta não está habilitado para este usuário.",
    };

    public override IdentityError UserAlreadyInRole(string role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = $"O usuário já pertence ao papel '{role}'.",
    };

    public override IdentityError UserNotInRole(string role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = $"O usuário não pertence ao papel '{role}'.",
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = $"A senha precisa ter pelo menos {length} caracteres.",
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = "A senha precisa ter pelo menos um caractere que não seja letra nem número.",
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = "A senha precisa ter pelo menos um número ('0'-'9').",
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = "A senha precisa ter pelo menos uma letra minúscula ('a'-'z').",
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = "A senha precisa ter pelo menos uma letra maiúscula ('A'-'Z').",
    };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = $"A senha precisa ter pelo menos {uniqueChars} caracteres distintos.",
    };
}
