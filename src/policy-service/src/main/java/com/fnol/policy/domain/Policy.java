package com.fnol.policy.domain;

import com.fnol.common.entity.BaseEntity;
import jakarta.persistence.*;
import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.List;

/**
 * Policy entity representing insurance policies
 */
@Entity
@Table(name = "policies", indexes = {
    @Index(name = "idx_policy_number", columnList = "policyNumber"),
    @Index(name = "idx_policy_status", columnList = "status"),
    @Index(name = "idx_policy_effective_date", columnList = "effectiveDate"),
    @Index(name = "idx_policy_expiry_date", columnList = "expiryDate")
})
public class Policy extends BaseEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @NotBlank
    @Size(max = 10)
    @Column(name = "policy_number", unique = true, nullable = false, length = 10)
    private String policyNumber;

    @NotBlank
    @Size(max = 2)
    @Column(name = "policy_version", nullable = false, length = 2)
    private String policyVersion = "01";

    @NotNull
    @Enumerated(EnumType.STRING)
    @Column(name = "status", nullable = false, length = 20)
    private PolicyStatus status = PolicyStatus.ACTIVE;

    @NotBlank
    @Size(max = 18)
    @Column(name = "product_type", nullable = false, length = 18)
    private String productType;

    @NotBlank
    @Size(max = 36)
    @Column(name = "policyholder_name", nullable = false, length = 36)
    private String policyholderName;

    @NotNull
    @DecimalMin(value = "0.0", inclusive = false)
    @Column(name = "coverage_amount", precision = 12, scale = 2, nullable = false)
    private BigDecimal coverageAmount;

    @NotNull
    @DecimalMin(value = "0.0")
    @Column(name = "deductible", precision = 12, scale = 2, nullable = false)
    private BigDecimal deductible;

    @NotNull
    @Column(name = "effective_date", nullable = false)
    private LocalDate effectiveDate;

    @NotNull
    @Column(name = "expiry_date", nullable = false)
    private LocalDate expiryDate;

    @Size(max = 100)
    @Column(name = "policyholder_email", length = 100)
    private String policyholderEmail;

    @Size(max = 20)
    @Column(name = "policyholder_phone", length = 20)
    private String policyholderPhone;

    @Column(name = "policyholder_address", columnDefinition = "TEXT")
    private String policyholderAddress;

    @OneToMany(mappedBy = "policy", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    private List<PolicyCoverage> coverages = new ArrayList<>();

    @OneToMany(mappedBy = "policy", cascade = CascadeType.ALL, fetch = FetchType.LAZY)
    private List<PolicyVersion> versions = new ArrayList<>();

    // Constructors
    public Policy() {}

    public Policy(String policyNumber, String productType, String policyholderName, 
                  BigDecimal coverageAmount, BigDecimal deductible, 
                  LocalDate effectiveDate, LocalDate expiryDate) {
        this.policyNumber = policyNumber;
        this.productType = productType;
        this.policyholderName = policyholderName;
        this.coverageAmount = coverageAmount;
        this.deductible = deductible;
        this.effectiveDate = effectiveDate;
        this.expiryDate = expiryDate;
    }

    // Getters and Setters
    public Long getId() {
        return id;
    }

    public void setId(Long id) {
        this.id = id;
    }

    public String getPolicyNumber() {
        return policyNumber;
    }

    public void setPolicyNumber(String policyNumber) {
        this.policyNumber = policyNumber;
    }

    public String getPolicyVersion() {
        return policyVersion;
    }

    public void setPolicyVersion(String policyVersion) {
        this.policyVersion = policyVersion;
    }

    public PolicyStatus getStatus() {
        return status;
    }

    public void setStatus(PolicyStatus status) {
        this.status = status;
    }

    public String getProductType() {
        return productType;
    }

    public void setProductType(String productType) {
        this.productType = productType;
    }

    public String getPolicyholderName() {
        return policyholderName;
    }

    public void setPolicyholderName(String policyholderName) {
        this.policyholderName = policyholderName;
    }

    public BigDecimal getCoverageAmount() {
        return coverageAmount;
    }

    public void setCoverageAmount(BigDecimal coverageAmount) {
        this.coverageAmount = coverageAmount;
    }

    public BigDecimal getDeductible() {
        return deductible;
    }

    public void setDeductible(BigDecimal deductible) {
        this.deductible = deductible;
    }

    public LocalDate getEffectiveDate() {
        return effectiveDate;
    }

    public void setEffectiveDate(LocalDate effectiveDate) {
        this.effectiveDate = effectiveDate;
    }

    public LocalDate getExpiryDate() {
        return expiryDate;
    }

    public void setExpiryDate(LocalDate expiryDate) {
        this.expiryDate = expiryDate;
    }

    public String getPolicyholderEmail() {
        return policyholderEmail;
    }

    public void setPolicyholderEmail(String policyholderEmail) {
        this.policyholderEmail = policyholderEmail;
    }

    public String getPolicyholderPhone() {
        return policyholderPhone;
    }

    public void setPolicyholderPhone(String policyholderPhone) {
        this.policyholderPhone = policyholderPhone;
    }

    public String getPolicyholderAddress() {
        return policyholderAddress;
    }

    public void setPolicyholderAddress(String policyholderAddress) {
        this.policyholderAddress = policyholderAddress;
    }

    public List<PolicyCoverage> getCoverages() {
        return coverages;
    }

    public void setCoverages(List<PolicyCoverage> coverages) {
        this.coverages = coverages;
    }

    public List<PolicyVersion> getVersions() {
        return versions;
    }

    public void setVersions(List<PolicyVersion> versions) {
        this.versions = versions;
    }

    // Helper methods
    public boolean isActive() {
        return status == PolicyStatus.ACTIVE && 
               LocalDate.now().isBefore(expiryDate) && 
               LocalDate.now().isAfter(effectiveDate.minusDays(1));
    }

    public boolean isExpired() {
        return LocalDate.now().isAfter(expiryDate);
    }
}