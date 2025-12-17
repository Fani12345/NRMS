namespace NRMS.Domain.Numbering;

public enum NumberType
{
    Unknown = 0,

    // E.164 national destination codes / MSISDN blocks (mobile numbers)
    Msisdn = 1,

    // E.164 country code + national numbers (general telephone numbering)
    E164 = 2,

    // Short codes (e.g., emergency, USSD, service codes) - modelled as numeric blocks
    ShortCode = 3
}
