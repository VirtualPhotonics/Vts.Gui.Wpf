using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Test.Extensions
{
    /// <summary>
    /// Tests EnumExtensions classe
    /// </summary>
    [TestFixture]
    public class EnumExtensionsTests
    {
        /// <summary>
        /// Verifies extension method IsGaussianForwardModel returns correct value
        /// </summary>
        [Test]
        public void verify_method_IsGaussianForwardModel_returns_correct_value()
        {
            Assert.AreEqual(ForwardSolverType.PointSourceSDA.IsGaussianForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.DeltaPOne.IsGaussianForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.MonteCarlo.IsGaussianForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.TwoLayerSDA.IsGaussianForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.DistributedGaussianSourceSDA.IsGaussianForwardModel(), true);
        }

        /// <summary>
        /// Verifies extension method IsMultiRegionForwardModel returns correct value
        /// </summary>
        [Test]
        public void verify_method_IsMultiRegionForwardModel_returns_correct_value()
        {
            Assert.AreEqual(ForwardSolverType.PointSourceSDA.IsMultiRegionForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.DeltaPOne.IsMultiRegionForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.MonteCarlo.IsMultiRegionForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.DistributedGaussianSourceSDA.IsMultiRegionForwardModel(), false);
            Assert.AreEqual(ForwardSolverType.TwoLayerSDA.IsMultiRegionForwardModel(), true);
        }

        /// <summary>
        /// Verifies extension method GetMaxArgumentLocation returns correct value
        /// </summary>
        [Test]
        public void verify_method_GetMaxArgumentLocation_returns_correct_value()
        {
            Assert.AreEqual(IndependentVariableAxis.Time.GetMaxArgumentLocation(), 2);
            Assert.AreEqual(IndependentVariableAxis.Rho.GetMaxArgumentLocation(), 0);
            Assert.AreEqual(IndependentVariableAxis.Fx.GetMaxArgumentLocation(), 0);
            Assert.AreEqual(IndependentVariableAxis.Ft.GetMaxArgumentLocation(), 2);
            Assert.AreEqual(IndependentVariableAxis.Z.GetMaxArgumentLocation(), 1);
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetMaxArgumentLocation(), 2);
        }

        /// <summary>
        /// Verifies extension method GetUnits for IndependentVariableAxis returns correct value
        /// </summary>
        [Test]
        public void verify_method_GetUnits_for_IndependentVariableAxis_returns_correct_value()
        {
            Assert.AreEqual(IndependentVariableAxis.Time.GetUnits(),
                IndependentVariableAxisUnits.NS.GetInternationalizedString());
            Assert.AreEqual(IndependentVariableAxis.Rho.GetUnits(), 
                IndependentVariableAxisUnits.MM.GetInternationalizedString());
            Assert.AreEqual(IndependentVariableAxis.Fx.GetUnits(),
                IndependentVariableAxisUnits.InverseMM.GetInternationalizedString());
            Assert.AreEqual(IndependentVariableAxis.Ft.GetUnits(),
                IndependentVariableAxisUnits.GHz.GetInternationalizedString());
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetUnits(),
                IndependentVariableAxisUnits.NM.GetInternationalizedString());
        }

        /// <summary>
        /// Verifies extension method GetTitle for IndependentVariableAxis returns correct value
        /// </summary>
        [Test]
        public void verify_method_GetTitle_for_IndependentVariableAxis_returns_correct_value()
        {
            Assert.AreEqual(IndependentVariableAxis.Rho.GetTitle(),
                IndependentVariableAxis.Rho.GetLocalizedString());
            Assert.AreEqual(IndependentVariableAxis.Time.GetTitle(),
                IndependentVariableAxis.Time.GetLocalizedString());
            Assert.AreEqual(IndependentVariableAxis.Fx.GetTitle(),
                IndependentVariableAxis.Fx.GetLocalizedString());
            Assert.AreEqual(IndependentVariableAxis.Ft.GetTitle(),
                IndependentVariableAxis.Ft.GetLocalizedString());
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetTitle(),
                IndependentVariableAxis.Wavelength.GetLocalizedString());
        }

        /// <summary>
        /// Verifies extension method GetUnits for SolutionDomainType returns correct value
        /// </summary>
        [Test]
        public void verify_method_GetUnits_for_SolutionDomainType_returns_correct_value()
        {
            Assert.AreEqual(SolutionDomainType.ROfRho.GetUnits(),
                DependentVariableAxisUnits.PerMMSquared.GetInternationalizedString());
            Assert.AreEqual(SolutionDomainType.ROfFx.GetUnits(),
                DependentVariableAxisUnits.Unitless.GetInternationalizedString());
            Assert.AreEqual(SolutionDomainType.ROfRhoAndTime.GetUnits(),
                DependentVariableAxisUnits.PerMMSquaredPerNS.GetInternationalizedString());
            Assert.AreEqual(SolutionDomainType.ROfFxAndTime.GetUnits(),
                DependentVariableAxisUnits.PerNS.GetInternationalizedString());
            Assert.AreEqual(SolutionDomainType.ROfRhoAndFt.GetUnits(),
                DependentVariableAxisUnits.PerMMSquaredPerGHz.GetInternationalizedString());
            Assert.AreEqual(SolutionDomainType.ROfFxAndFt.GetUnits(),
                DependentVariableAxisUnits.PerGHz.GetInternationalizedString());
        }

        /// <summary>
        /// Verifies extension method GetUnits for FluenceSolutionDomainType returns correct value
        /// </summary>
        [Test]
        public void verify_method_GetUnits_for_FluenceSolutionDomainType_returns_correct_value()
        {
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfRhoAndZ.GetUnits(),
                DependentVariableAxisUnits.PerMMCubed.GetInternationalizedString());
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfFxAndZ.GetUnits(),
                DependentVariableAxisUnits.PerMM.GetInternationalizedString());
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfRhoAndZAndTime.GetUnits(),
                DependentVariableAxisUnits.PerMMCubedPerNS.GetInternationalizedString());
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfFxAndZAndTime.GetUnits(),
                DependentVariableAxisUnits.PerMMPerNS.GetInternationalizedString());
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfRhoAndZAndFt.GetUnits(),
                DependentVariableAxisUnits.PerMMCubedPerGHz.GetInternationalizedString());
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfFxAndZAndFt.GetUnits(),
                DependentVariableAxisUnits.PerMMPerGHz.GetInternationalizedString());
        }


        /// <summary>
        /// Verifies extension method GetDefaultRange returns correct value
        /// </summary>
        [Test]
        public void verify_method_GetDefaultRange_returns_correct_value()
        {
            Assert.AreEqual(IndependentVariableAxis.Time.GetDefaultRange().Start, 0);
            Assert.AreEqual(IndependentVariableAxis.Time.GetDefaultRange().Stop, 0.05);
            Assert.AreEqual(IndependentVariableAxis.Time.GetDefaultRange().Count, 51);
            Assert.AreEqual(IndependentVariableAxis.Rho.GetDefaultRange().Start, 0.5);
            Assert.AreEqual(IndependentVariableAxis.Rho.GetDefaultRange().Stop, 9.5);
            Assert.AreEqual(IndependentVariableAxis.Rho.GetDefaultRange().Count, 19);
            Assert.AreEqual(IndependentVariableAxis.Fx.GetDefaultRange().Start, 0);
            Assert.AreEqual(IndependentVariableAxis.Fx.GetDefaultRange().Stop, 0.5);
            Assert.AreEqual(IndependentVariableAxis.Fx.GetDefaultRange().Count, 51);
            Assert.AreEqual(IndependentVariableAxis.Ft.GetDefaultRange().Start, 0);
            Assert.AreEqual(IndependentVariableAxis.Ft.GetDefaultRange().Stop, 0.5);
            Assert.AreEqual(IndependentVariableAxis.Ft.GetDefaultRange().Count, 51);
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetDefaultRange().Start, 650);
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetDefaultRange().Stop, 1000);
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetDefaultRange().Count, 36);
        }


        /// <summary>
        /// Verifies extension method GetDefaultConstantAxisValue returns correct value
        /// </summary>
        [Test]
        public void verify_method_GetDefaultConstantAxisValue_returns_correct_value()
        {
            Assert.AreEqual(IndependentVariableAxis.Time.GetDefaultConstantAxisValue(), 0.05);
            Assert.AreEqual(IndependentVariableAxis.Rho.GetDefaultConstantAxisValue(), 1);
            Assert.AreEqual(IndependentVariableAxis.Fx.GetDefaultConstantAxisValue(), 0.0);
            Assert.AreEqual(IndependentVariableAxis.Ft.GetDefaultConstantAxisValue(), 0.0);
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetDefaultConstantAxisValue(), 650.0);
        }
    }
}
