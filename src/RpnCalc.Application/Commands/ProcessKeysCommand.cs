using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;

namespace RpnCalc.Application.Commands;

public sealed record ProcessKeysCommand(
    IReadOnlyList<string> Keys,
    ExpressionMode Mode,
    bool ReturnTrace,
    CalcSettings? Settings);

public sealed class ProcessKeysCommandHandler
{
    private readonly KeyStreamInterpreter _interpreter;
    private readonly EvaluateExpressionCommandHandler _evaluator;

    public ProcessKeysCommandHandler(KeyStreamInterpreter interpreter, EvaluateExpressionCommandHandler evaluator)
    {
        _interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public EvaluationResult Handle(ProcessKeysCommand command)
    {
        CalculatorMode mode = command.Mode == ExpressionMode.Infix ? CalculatorMode.Infix : CalculatorMode.Rpn;
        KeyStreamResult streamResult = _interpreter.Interpret(command.Keys, mode);
        if (string.IsNullOrWhiteSpace(streamResult.Expression))
        {
            return new EvaluationResult(0m, Array.Empty<Token>(), Array.Empty<string>());
        }

        EvaluateExpressionCommand evaluationCommand = new EvaluateExpressionCommand(streamResult.Expression, command.Mode, command.ReturnTrace, command.Settings);
        return _evaluator.Handle(evaluationCommand);
    }
}
