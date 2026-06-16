namespace NeuroViva.Domain.Exceptions;

public sealed class BusinessRuleViolationException : DomainException
{
    public string RuleCode { get; }

    public BusinessRuleViolationException(string ruleCode, string message)
        : base(message)
    {
        RuleCode = ruleCode;
    }
}
