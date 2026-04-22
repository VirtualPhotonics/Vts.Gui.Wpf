using System;
using Vts.Common;

namespace Vts.Gui.Wpf.Extensions;

public static class EnumExtensions
{
    private const string CustomNotImplementedException = "Exception_CustomNotImplemented";
    private const string IndependentAxisException = "Exception_IndependentAxis";

    extension(ForwardSolverType forwardSolverType)
    {
        public bool IsGaussianForwardModel()
        {
            return forwardSolverType switch
            {
                ForwardSolverType.DistributedGaussianSourceSDA => true,
                _ => false,
            };
        }

        public bool IsMultiRegionForwardModel()
        {
            return forwardSolverType switch
            {
                ForwardSolverType.TwoLayerSDA => true,
                _ => false,
            };
        }
    }

    extension(IndependentVariableAxis axis)
    {
        public int GetMaxArgumentLocation()
        {
            return axis switch
            {
                IndependentVariableAxis.Rho => 0,
                IndependentVariableAxis.Time => 2,
                IndependentVariableAxis.Fx => 0,
                IndependentVariableAxis.Ft => 2,
                IndependentVariableAxis.Z => 1,
                IndependentVariableAxis.Wavelength => 2,
                _ => throw new NotImplementedException(
                    StringLookup.GetLocalizedString(IndependentAxisException) +
                    axis + 
                    StringLookup.GetLocalizedString(CustomNotImplementedException))
            };
        }

        public bool IsSpatialAxis()
        {
            return axis switch
            {
                IndependentVariableAxis.Rho or IndependentVariableAxis.Fx => true,
                IndependentVariableAxis.Time or IndependentVariableAxis.Ft or IndependentVariableAxis.Z
                    or IndependentVariableAxis.Wavelength => false,
                _ => throw new NotImplementedException(
                    StringLookup.GetLocalizedString(IndependentAxisException) + 
                    axis + 
                    StringLookup.GetLocalizedString(CustomNotImplementedException))
            };
        }

        public bool IsTemporalAxis()
        {
            return axis switch
            {
                IndependentVariableAxis.Time or IndependentVariableAxis.Ft => true,
                IndependentVariableAxis.Rho or IndependentVariableAxis.Fx or IndependentVariableAxis.Z
                    or IndependentVariableAxis.Wavelength => false,
                _ => throw new NotImplementedException(
                    StringLookup.GetLocalizedString(IndependentAxisException) +
                    axis + 
                    StringLookup.GetLocalizedString(CustomNotImplementedException))
            };
        }

        public bool IsDepthAxis()
        {
            return axis switch
            {
                IndependentVariableAxis.Z => true,
                IndependentVariableAxis.Time or IndependentVariableAxis.Ft or IndependentVariableAxis.Rho
                    or IndependentVariableAxis.Fx or IndependentVariableAxis.Wavelength => false,
                _ => throw new NotImplementedException(
                    StringLookup.GetLocalizedString(IndependentAxisException) +
                    axis + 
                    StringLookup.GetLocalizedString(CustomNotImplementedException))
            };
        }

        public bool IsWavelengthAxis()
        {
            return axis switch
            {
                IndependentVariableAxis.Wavelength => true,
                IndependentVariableAxis.Time or IndependentVariableAxis.Ft or IndependentVariableAxis.Rho or IndependentVariableAxis.Fx or IndependentVariableAxis.Z => false,
                _ => throw new NotImplementedException(
                    StringLookup.GetLocalizedString(IndependentAxisException) + 
                    axis +
                    StringLookup.GetLocalizedString(CustomNotImplementedException))
            };
        }

        public string GetTitle()
        {
            return axis switch
            {
                IndependentVariableAxis.Time => IndependentVariableAxis.Time.GetLocalizedString(),
                IndependentVariableAxis.Fx => IndependentVariableAxis.Fx.GetLocalizedString(),
                IndependentVariableAxis.Ft => IndependentVariableAxis.Ft.GetLocalizedString(),
                IndependentVariableAxis.Wavelength => IndependentVariableAxis.Wavelength.GetLocalizedString(),
                _ => IndependentVariableAxis.Rho.GetLocalizedString(),
            };
        }

        public string GetUnits()
        {
            return axis switch
            {
                IndependentVariableAxis.Time => IndependentVariableAxisUnits.NS.GetInternationalizedString(),
                IndependentVariableAxis.Fx => StringLookup.GetLocalizedString("Measurement_Inv_mm"),
                IndependentVariableAxis.Ft => IndependentVariableAxisUnits.GHz.GetInternationalizedString(),
                IndependentVariableAxis.Wavelength => IndependentVariableAxisUnits.NM.GetInternationalizedString(),
                _ => IndependentVariableAxisUnits.MM.GetInternationalizedString(),
            };
        }
    }

    public static string GetUnits(this SolutionDomainType sdType)
    {
        return sdType switch
        {
            SolutionDomainType.ROfFx => DependentVariableAxisUnits.Unitless.GetInternationalizedString(),
            SolutionDomainType.ROfRhoAndTime => DependentVariableAxisUnits.PerMMSquaredPerNS.GetInternationalizedString(),
            SolutionDomainType.ROfFxAndTime => DependentVariableAxisUnits.PerNS.GetInternationalizedString(),
            SolutionDomainType.ROfRhoAndFt => DependentVariableAxisUnits.PerMMSquaredPerGHz.GetInternationalizedString(),
            SolutionDomainType.ROfFxAndFt => DependentVariableAxisUnits.PerGHz.GetInternationalizedString(),
            _ => DependentVariableAxisUnits.PerMMSquared.GetInternationalizedString(),
        };
    }

    public static string GetUnits(this FluenceSolutionDomainType sdType)
    {
        return sdType switch
        {
            FluenceSolutionDomainType.FluenceOfFxAndZ => DependentVariableAxisUnits.PerMM.GetInternationalizedString(),
            FluenceSolutionDomainType.FluenceOfRhoAndZAndTime => DependentVariableAxisUnits.PerMMCubedPerNS.GetInternationalizedString(),
            FluenceSolutionDomainType.FluenceOfFxAndZAndTime => DependentVariableAxisUnits.PerMMPerNS.GetInternationalizedString(),
            FluenceSolutionDomainType.FluenceOfRhoAndZAndFt => DependentVariableAxisUnits.PerMMCubedPerGHz.GetInternationalizedString(),
            FluenceSolutionDomainType.FluenceOfFxAndZAndFt => DependentVariableAxisUnits.PerMMPerGHz.GetInternationalizedString(),
            _ => DependentVariableAxisUnits.PerMMCubed.GetInternationalizedString(),
        };
    }

    extension(IndependentVariableAxis independentAxisType)
    {
        public DoubleRange GetDefaultRange()
        {
            return independentAxisType switch
            {
                IndependentVariableAxis.Time => new DoubleRange(0D, 0.05D, 51),// units=ns
                IndependentVariableAxis.Fx => new DoubleRange(0D, 0.5D, 51),
                IndependentVariableAxis.Ft => new DoubleRange(0D, 0.5D, 51),// units=GHz
                IndependentVariableAxis.Wavelength => new DoubleRange(650D, 1000D, 36),// units=nm
                _ => new DoubleRange(0.5D, 9.5D, 19),// units=mm
            };
        }

        public double GetDefaultConstantAxisValue()
        {
            return independentAxisType switch
            {
                IndependentVariableAxis.Time => 0.05,
                IndependentVariableAxis.Fx => 0.0,
                IndependentVariableAxis.Ft => 0.0,
                IndependentVariableAxis.Wavelength => 650.0,
                _ => 1.0,
            };
        }
    }
}