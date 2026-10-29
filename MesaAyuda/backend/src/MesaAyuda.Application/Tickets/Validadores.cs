using FluentValidation;

namespace MesaAyuda.Application.Tickets;

public class CrearTicketValidador : AbstractValidator<CrearTicketDto>
{
    public CrearTicketValidador()
    {
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(150);
        RuleFor(x => x.Descripcion).NotEmpty().WithMessage("Describe el problema.")
            .MinimumLength(15).WithMessage("Describe el problema con más detalle (mínimo 15 caracteres).")
            .MaximumLength(2000);
        RuleFor(x => x.Solicitante).NotEmpty().WithMessage("Indica quién reporta el problema.")
            .MaximumLength(100);
        RuleFor(x => x.Categoria).IsInEnum();
        RuleFor(x => x.Prioridad).IsInEnum();
    }
}

public class ComentarioValidador : AbstractValidator<ComentarioDto>
{
    public ComentarioValidador()
    {
        RuleFor(x => x.Autor).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(1000);
    }
}
