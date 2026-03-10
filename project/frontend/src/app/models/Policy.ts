export interface Policy {
    id?: number | string;
    name: string;
    healthCoverage: number;
    lifeCoverageMultiplier: number;
    maxLifeCoverageLimit: number;
    accidentCoverage: number;
    premiumPerEmployee: number;
    minEmployees: number;
    durationYears: number;
    isPopular: boolean;
    isActive?: boolean;
}
