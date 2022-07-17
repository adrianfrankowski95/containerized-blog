using System;
using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Emailing.API.Models;

public record Recipient
{
    public string EmailAddress { get; init; }
    public string Name { get; init; }
    public Recipient(string emailAddress, string name)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            throw new ArgumentNullException(nameof(emailAddress));

        if (!new EmailAddressAttribute().IsValid(emailAddress))
            throw new FormatException(nameof(emailAddress));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        EmailAddress = emailAddress;
        Name = name;
    }
}