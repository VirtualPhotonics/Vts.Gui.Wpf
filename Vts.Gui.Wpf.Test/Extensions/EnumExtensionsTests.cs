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
        public void Verify_method_IsGaussianForwardModel_returns_correct_value()
        {
            Assert.IsFalse(ForwardSolverType.PointSourceSDA.IsGaussianForwardModel());
            Assert.IsFalse(ForwardSolverType.DeltaPOne.IsGaussianForwardModel());
            Assert.IsFalse(ForwardSolverType.MonteCarlo.IsGaussianForwardModel());
            Assert.IsFalse(ForwardSolverType.TwoLayerSDA.IsGaussianForwardModel());
            Assert.IsTrue(ForwardSolverType.DistributedGaussianSourceSDA.IsGaussianForwardModel());
        }

        /// <summary>
        /// Verifies extension method IsMultiRegionForwardModel returns correct value
        /// </summary>
        [Test]
        public void Verify_method_IsMultiRegionForwardModel_returns_correct_value()
        {
            Assert.IsFalse(ForwardSolverType.PointSourceSDA.IsMultiRegionForwardModel());
            Assert.IsFalse(ForwardSolverType.DeltaPOne.IsMultiRegionForwardModel());
            Assert.IsFalse(ForwardSolverType.MonteCarlo.IsMultiRegionForwardModel());
            Assert.IsFalse(ForwardSolverType.DistributedGaussianSourceSDA.IsMultiRegionForwardModel());
            Assert.IsTrue(ForwardSolverType.TwoLayerSDA.IsMultiRegionForwardModel());
        }

        /// <summary>
        /// Verifies extension method GetMaxArgumentLocation returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetMaxArgumentLocation_returns_correct_value()
        {
            Assert.AreEqual(2,IndependentVariableAxis.Time.GetMaxArgumentLocation());
            Assert.AreEqual(0, IndependentVariableAxis.Rho.GetMaxArgumentLocation());
            Assert.AreEqual(0, IndependentVariableAxis.Fx.GetMaxArgumentLocation());
            Assert.AreEqual(2, IndependentVariableAxis.Ft.GetMaxArgumentLocation());
            Assert.AreEqual(1, IndependentVariableAxis.Z.GetMaxArgumentLocation());
            Assert.AreEqual(2, IndependentVariableAxis.Wavelength.GetMaxArgumentLocation());
        }

        /// <summary>
        /// Verifies extension method GetUnits for IndependentVariableAxis returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetUnits_for_IndependentVariableAxis_returns_correct_value()
        {
            Assert.AreEqual(IndependentVariableAxisUnits.NS.GetInternationalizedString(), IndependentVariableAxis.Time.GetUnits());
            Assert.AreEqual(IndependentVariableAxisUnits.MM.GetInternationalizedString(), IndependentVariableAxis.Rho.GetUnits());
            Assert.AreEqual(IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(), IndependentVariableAxis.Fx.GetUnits());
            Assert.AreEqual(IndependentVariableAxisUnits.GHz.GetInternationalizedString(), IndependentVariableAxis.Ft.GetUnits());
            Assert.AreEqual(IndependentVariableAxisUnits.NM.GetInternationalizedString(), IndependentVariableAxis.Wavelength.GetUnits());
        }

        /// <summary>
        /// Verifies extension method GetTitle for IndependentVariableAxis returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetTitle_for_IndependentVariableAxis_returns_correct_value()
        {
            Assert.AreEqual(IndependentVariableAxis.Rho.GetLocalizedString(), IndependentVariableAxis.Rho.GetTitle());
            Assert.AreEqual(IndependentVariableAxis.Time.GetLocalizedString(), IndependentVariableAxis.Time.GetTitle());
            Assert.AreEqual(IndependentVariableAxis.Fx.GetLocalizedString(), IndependentVariableAxis.Fx.GetTitle());
            Assert.AreEqual(IndependentVariableAxis.Ft.GetLocalizedString(), IndependentVariableAxis.Ft.GetTitle());
            Assert.AreEqual(IndependentVariableAxis.Wavelength.GetLocalizedString(), IndependentVariableAxis.Wavelength.GetTitle());
        }

        /// <summary>
        /// Verifies extension method GetUnits for SolutionDomainType returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetUnits_for_SolutionDomainType_returns_correct_value()
        {
            Assert.AreEqual(DependentVariableAxisUnits.PerMMSquared.GetInternationalizedString(), SolutionDomainType.ROfRho.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.Unitless.GetInternationalizedString(), SolutionDomainType.ROfFx.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerMMSquaredPerNS.GetInternationalizedString(), SolutionDomainType.ROfRhoAndTime.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerNS.GetInternationalizedString(), SolutionDomainType.ROfFxAndTime.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerMMSquaredPerGHz.GetInternationalizedString(), SolutionDomainType.ROfRhoAndFt.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerGHz.GetInternationalizedString(), SolutionDomainType.ROfFxAndFt.GetUnits());
        }

        /// <summary>
        /// Verifies extension method GetUnits for FluenceSolutionDomainType returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetUnits_for_FluenceSolutionDomainType_returns_correct_value()
        {
            Assert.AreEqual(DependentVariableAxisUnits.PerMMCubed.GetInternationalizedString(), FluenceSolutionDomainType.FluenceOfRhoAndZ.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerMM.GetInternationalizedString(), FluenceSolutionDomainType.FluenceOfFxAndZ.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerMMCubedPerNS.GetInternationalizedString(), FluenceSolutionDomainType.FluenceOfRhoAndZAndTime.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerMMPerNS.GetInternationalizedString(), FluenceSolutionDomainType.FluenceOfFxAndZAndTime.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerMMCubedPerGHz.GetInternationalizedString(), FluenceSolutionDomainType.FluenceOfRhoAndZAndFt.GetUnits());
            Assert.AreEqual(DependentVariableAxisUnits.PerMMPerGHz.GetInternationalizedString(), FluenceSolutionDomainType.FluenceOfFxAndZAndFt.GetUnits());
        }


        /// <summary>
        /// Verifies extension method GetDefaultRange returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetDefaultRange_returns_correct_value()
        {
            Assert.AreEqual(0, IndependentVariableAxis.Time.GetDefaultRange().Start);
            Assert.AreEqual(0.05, IndependentVariableAxis.Time.GetDefaultRange().Stop);
            Assert.AreEqual(51, IndependentVariableAxis.Time.GetDefaultRange().Count);
            Assert.AreEqual(0.5, IndependentVariableAxis.Rho.GetDefaultRange().Start);
            Assert.AreEqual(9.5, IndependentVariableAxis.Rho.GetDefaultRange().Stop);
            Assert.AreEqual(19, IndependentVariableAxis.Rho.GetDefaultRange().Count);
            Assert.AreEqual(0, IndependentVariableAxis.Fx.GetDefaultRange().Start);
            Assert.AreEqual(0.5, IndependentVariableAxis.Fx.GetDefaultRange().Stop);
            Assert.AreEqual(51, IndependentVariableAxis.Fx.GetDefaultRange().Count);
            Assert.AreEqual(0, IndependentVariableAxis.Ft.GetDefaultRange().Start);
            Assert.AreEqual(0.5, IndependentVariableAxis.Ft.GetDefaultRange().Stop);
            Assert.AreEqual(51, IndependentVariableAxis.Ft.GetDefaultRange().Count);
            Assert.AreEqual(650, IndependentVariableAxis.Wavelength.GetDefaultRange().Start);
            Assert.AreEqual(1000, IndependentVariableAxis.Wavelength.GetDefaultRange().Stop);
            Assert.AreEqual(36, IndependentVariableAxis.Wavelength.GetDefaultRange().Count);
        }


        /// <summary>
        /// Verifies extension method GetDefaultConstantAxisValue returns correct value
        /// </summary>
        [Test]
        public void Verify_method_GetDefaultConstantAxisValue_returns_correct_value()
        {
            Assert.AreEqual(0.05, IndependentVariableAxis.Time.GetDefaultConstantAxisValue());
            Assert.AreEqual(1, IndependentVariableAxis.Rho.GetDefaultConstantAxisValue());
            Assert.AreEqual(0.0, IndependentVariableAxis.Fx.GetDefaultConstantAxisValue());
            Assert.AreEqual(0.0, IndependentVariableAxis.Ft.GetDefaultConstantAxisValue());
            Assert.AreEqual(650.0, IndependentVariableAxis.Wavelength.GetDefaultConstantAxisValue());
        }
    }
}
