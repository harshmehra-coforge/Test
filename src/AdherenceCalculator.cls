/** Calculates bounded rolling adherence and consecutive missed-dose signals. */
public with sharing class AdherenceCalculator {
    public static DoseLogDtos.AdherenceResult calculate(Id carePlanId) {
        // TODO: Query schedule occurrences and dose logs for bounded 90-day windows.
        throw new DoseLogException('Adherence calculation is not implemented.');
    }

    public class DoseLogException extends Exception {}
}
