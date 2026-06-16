using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class GuestService
{
    private const string MissingRequiredFieldsCode = "MISSING_REQUIRED_FIELDS";
    private const string MissingRequiredFieldsMessage = "Debes completar todos los campos obligatorios del huésped.";
    private const string InvalidEmailCode = "INVALID_EMAIL";
    private const string InvalidEmailMessage = "El email ingresado no tiene un formato válido.";
    private const string DuplicateDocumentCode = "DUPLICATE_DOCUMENT";
    private const string DuplicateDocumentMessage = "Ya existe un huésped con el mismo tipo y número de documento en ese país.";

    private readonly GuestRepository _repository;

    public GuestService(GuestRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Guest>> GetAllGuestsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Guest?> GetGuestByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<OperationResult<Guest>> RegisterGuestAsync(GuestRegistrationRequest request)
    {
        if (!HasRequiredFields(request))
        {
            return OperationResult<Guest>.Failure(MissingRequiredFieldsCode, MissingRequiredFieldsMessage);
        }

        var normalizedEmail = NormalizeOptionalValue(request.Email);
        if (!IsValidOptionalEmail(normalizedEmail))
        {
            return OperationResult<Guest>.Failure(InvalidEmailCode, InvalidEmailMessage);
        }

        var allGuests = await _repository.GetAllAsync();
        var existsDuplicate = allGuests.Any(g =>
            g.DocumentType == request.DocumentType.Trim() &&
            g.DocumentId == request.DocumentId.Trim() &&
            g.Country == request.Country.Trim());

        if (existsDuplicate)
        {
            return OperationResult<Guest>.Failure(DuplicateDocumentCode, DuplicateDocumentMessage);
        }

        var guest = BuildGuest(request, normalizedEmail);

        var created = await _repository.AddAsync(guest);
        return OperationResult<Guest>.Success(created, "Huésped registrado correctamente.");
    }

    private static bool HasRequiredFields(GuestRegistrationRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.FirstName) &&
            !string.IsNullOrWhiteSpace(request.LastName) &&
            !string.IsNullOrWhiteSpace(request.DocumentType) &&
            !string.IsNullOrWhiteSpace(request.DocumentId) &&
            !string.IsNullOrWhiteSpace(request.Country);
    }

    private static bool IsValidOptionalEmail(string? email)
    {
        if (email is null)
        {
            return true;
        }

        var emailValidator = new EmailAddressAttribute();
        return emailValidator.IsValid(email);
    }

    private static Guest BuildGuest(GuestRegistrationRequest request, string? normalizedEmail)
    {
        return new Guest
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DocumentType = request.DocumentType.Trim(),
            DocumentId = request.DocumentId.Trim(),
            Email = normalizedEmail,
            Phone = NormalizeOptionalValue(request.Phone),
            Country = request.Country.Trim(),
            CreatedAt = DateTime.Now
        };
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
