namespace JobTracker.Models;

// enum = sabit değerler kümesi. TypeScript'teki union type'ın karşılığı:
// type Status = "Applied" | "Interview" | "Rejected" | "Offer"
public enum ApplicationStatus
{
    Applied,    // Başvuruldu
    Interview,  // Mülakat
    Rejected,   // Ret
    Offer       // Teklif
}