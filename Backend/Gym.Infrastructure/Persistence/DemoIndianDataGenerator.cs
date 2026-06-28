namespace Gym.Infrastructure.Persistence;

/// <summary>Generates realistic Indian demo identities for MVP seeding.</summary>
internal static class DemoIndianDataGenerator
{
    private static readonly string[] MaleFirstNames =
    [
        "Aarav", "Vivaan", "Arjun", "Rohan", "Karan", "Aditya", "Nikhil", "Rahul", "Siddharth", "Varun",
        "Amit", "Suresh", "Rajesh", "Vikram", "Manoj", "Deepak", "Sanjay", "Prakash", "Anil", "Rakesh"
    ];

    private static readonly string[] FemaleFirstNames =
    [
        "Ananya", "Priya", "Neha", "Pooja", "Kavya", "Isha", "Meera", "Sneha", "Divya", "Ritu",
        "Anjali", "Shreya", "Nisha", "Kiran", "Lakshmi", "Sunita", "Rekha", "Geeta", "Swati", "Asha"
    ];

    private static readonly string[] Surnames =
    [
        "Sharma", "Patel", "Singh", "Kumar", "Reddy", "Iyer", "Nair", "Gupta", "Mehta", "Joshi",
        "Desai", "Shah", "Rao", "Pillai", "Chopra", "Malhotra", "Verma", "Kulkarni", "Bose", "Das"
    ];

    private static readonly (string City, string State, string Pin)[] Locations =
    [
        ("Mumbai", "Maharashtra", "400050"),
        ("Pune", "Maharashtra", "411001"),
        ("Bengaluru", "Karnataka", "560001"),
        ("Hyderabad", "Telangana", "500001"),
        ("Chennai", "Tamil Nadu", "600001"),
        ("Ahmedabad", "Gujarat", "380001"),
        ("Jaipur", "Rajasthan", "302001"),
        ("Kolkata", "West Bengal", "700001"),
        ("Lucknow", "Uttar Pradesh", "226001"),
        ("Indore", "Madhya Pradesh", "452001")
    ];

    private static readonly string[] Streets =
    [
        "Linking Road", "MG Road", "Park Street", "Brigade Road", "FC Road",
        "Anna Salai", "Ring Road", "Station Road", "Civil Lines", "Sector 18"
    ];

    private static readonly string[] TrainerSpecializations =
    [
        "Strength & Conditioning",
        "Yoga & Flexibility",
        "CrossFit & Functional Training",
        "Cardio & Weight Loss",
        "Bodybuilding & Hypertrophy"
    ];

    public static readonly string[] LeadSources =
    ["Walk-in", "Instagram", "Google", "Referral", "WhatsApp", "Justdial"];

    public static string MemberLoginId(int index) => $"fitzone_member{index:D3}";

    public static string TrainerLoginId(int index) => $"fitzone_trainer{index}";

    public static PersonProfile GenerateMember(int index, Random random)
    {
        var isMale = index % 3 != 0;
        var first = isMale
            ? MaleFirstNames[index % MaleFirstNames.Length]
            : FemaleFirstNames[index % FemaleFirstNames.Length];
        var last = Surnames[(index * 7) % Surnames.Length];
        var fullName = $"{first} {last}";
        var loc = Locations[index % Locations.Length];
        var street = Streets[(index * 3) % Streets.Length];
        var phone = GenerateMobile(index, random);
        var email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}{index}@fitzonegym.in";
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-(22 + index % 28)).AddDays(-index));
        var gender = isMale ? "Male" : "Female";
        var address = $"Flat {100 + index % 90}, {street}, {loc.City}, {loc.State} {loc.Pin}";
        var emergency = $"{FemaleFirstNames[(index + 5) % FemaleFirstNames.Length]} {last} ({GenerateMobile(index + 500, random)})";

        return new PersonProfile(fullName, MemberLoginId(index), email, phone, gender, dob, address, emergency, loc.City, loc.State, loc.Pin);
    }

    public static PersonProfile GenerateTrainer(int index, Random random)
    {
        var isMale = index % 2 == 1;
        var first = isMale
            ? MaleFirstNames[(index * 3) % MaleFirstNames.Length]
            : FemaleFirstNames[(index * 2) % FemaleFirstNames.Length];
        var last = Surnames[(index * 11) % Surnames.Length];
        var fullName = $"{first} {last}";
        var loc = Locations[index % Locations.Length];
        var phone = GenerateMobile(1000 + index, random);
        var email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}@fitzonegym.in";
        var specialization = TrainerSpecializations[(index - 1) % TrainerSpecializations.Length];
        var bio = $"{specialization} coach with {(5 + index * 2)} years of experience across {loc.City} fitness centres.";

        return new PersonProfile(
            fullName,
            TrainerLoginId(index),
            email,
            phone,
            isMale ? "Male" : "Female",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-(28 + index * 3))),
            $"#{index}, {loc.City}, {loc.State}",
            $"{last} Family ({phone})",
            loc.City,
            loc.State,
            loc.Pin,
            specialization,
            bio);
    }

    public static PersonProfile GenerateLead(int index, Random random)
    {
        var member = GenerateMember(200 + index, random);
        return member with
        {
            LoginIdentifier = string.Empty,
            Email = index % 2 == 0 ? member.Email : null
        };
    }

    public static string GenerateMobile(int seed, Random random)
    {
        var prefixes = new[] { "98", "97", "96", "95", "94", "93", "91", "90", "89", "88" };
        var prefix = prefixes[Math.Abs(seed) % prefixes.Length];
        var suffix = (Math.Abs(seed * 7919 + random.Next(1000, 9999)) % 100000000).ToString("D8");
        return $"+91-{prefix}{suffix}";
    }

    internal sealed record PersonProfile(
        string FullName,
        string LoginIdentifier,
        string? Email,
        string Phone,
        string Gender,
        DateOnly DateOfBirth,
        string Address,
        string EmergencyContact,
        string City,
        string State,
        string PinCode,
        string? Specialization = null,
        string? Bio = null);
}
