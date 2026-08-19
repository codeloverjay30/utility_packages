using System;
using System.Collections.Generic;
using AntiHijackUtilityServices.Abstractions;
using AntiHijackUtilityServices.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace AntiHijackUtilityService.Tests;

public class AntiHijackCoordinatorTests
{
    private readonly Mock<IOSPlatformValidator> _mockValidator;
    private readonly List<ISafetySensor> _registeredSensors;

    public AntiHijackCoordinatorTests()
    {
        // Defensively configuring Mock to prevent cascading side-effects or implicit invocation structures
        _mockValidator = new Mock<IOSPlatformValidator>(MockBehavior.Strict);
        _registeredSensors = new List<ISafetySensor>();
    }

    [Fact]
    public void VerifyEcosystemHealth_WhenPlatformFails_ShouldThrowPlatformNotSupportedException_ValidatedWithFluentAssertions()
    {
        // Arrange
        _mockValidator
            .Setup(v => v.ValidateOS())
            .Throws(new PlatformNotSupportedException("This application only supports the Windows operating system."));

        var coordinator = new AntiHijackCoordinator(_mockValidator.Object, _registeredSensors);

        // Act
        Action act = () => coordinator.VerifyEcosystemHealth();

        // Assert - STRICT REFILL: No xUnit native Assert allowed. Using FluentAssertions with explicit string mapping.
        act.Should()
           .Throw<PlatformNotSupportedException>()
           .WithMessage("*only supports the Windows operating system.*");
           
        _mockValidator.Verify(v => v.ValidateOS(), Times.Once);
    }

    [Fact]
    public void VerifyEcosystemHealth_WhenAnySensorDetectsThreat_ShouldReturnFalseImmediately()
    {
        // Arrange
        _mockValidator.Setup(v => v.ValidateOS()); // Setup empty callback successfully

        var cleanSensorMock = new Mock<ISafetySensor>(MockBehavior.Strict);
        cleanSensorMock.Setup(s => s.IsThreatDetected()).Returns(false);
        cleanSensorMock.Setup(s => s.SensorName).Returns("CleanSensor");

        var dirtySensorMock = new Mock<ISafetySensor>(MockBehavior.Strict);
        dirtySensorMock.Setup(s => s.IsThreatDetected()).Returns(true);
        dirtySensorMock.Setup(s => s.SensorName).Returns("CompromisedSensor");

        _registeredSensors.Add(cleanSensorMock.Object);
        _registeredSensors.Add(dirtySensorMock.Object);

        var coordinator = new AntiHijackCoordinator(_mockValidator.Object, _registeredSensors);

        // Act
        bool overallResult = coordinator.VerifyEcosystemHealth();

        // Assert
        overallResult.Should().BeFalse();
    }

    [Fact]
    public void VerifyEcosystemHealth_WhenAllSensorsAreClean_ShouldReturnTrue()
    {
        // Arrange
        _mockValidator.Setup(v => v.ValidateOS());

        var sensorMock1 = new Mock<ISafetySensor>(MockBehavior.Strict);
        sensorMock1.Setup(s => s.IsThreatDetected()).Returns(false);
        sensorMock1.Setup(s => s.SensorName).Returns("CleanSensor");
        var sensorMock2 = new Mock<ISafetySensor>(MockBehavior.Strict);
        sensorMock2.Setup(s => s.IsThreatDetected()).Returns(false);
        sensorMock2.Setup(s => s.SensorName).Returns("CleanSensor");

        _registeredSensors.Add(sensorMock1.Object);
        _registeredSensors.Add(sensorMock2.Object);

        var coordinator = new AntiHijackCoordinator(_mockValidator.Object, _registeredSensors);

        // Act
        bool overallResult = coordinator.VerifyEcosystemHealth();

        // Assert
        overallResult.Should().BeTrue();
    }

    [Fact]
    public void VerifyEcosystemHealth_WhenSensorDetectsThreat_ShouldShortCircuitImmediately_AndNotEvaluateSubsequentSensors()
    {
        // Arrange
        _mockValidator.Setup(v => v.ValidateOS());

        var compromisedSensorMock = new Mock<ISafetySensor>(MockBehavior.Strict);
        compromisedSensorMock.Setup(s => s.IsThreatDetected()).Returns(true); // 偵測到威脅
        compromisedSensorMock.Setup(s => s.SensorName).Returns("CompromisedSensor");

        // 如果發生短路，這個感測器的 IsThreatDetected 絕對不應該被呼叫
        var unexpectedSensorMock = new Mock<ISafetySensor>(MockBehavior.Strict);

        _registeredSensors.Add(compromisedSensorMock.Object);
        _registeredSensors.Add(unexpectedSensorMock.Object);

        var coordinator = new AntiHijackCoordinator(_mockValidator.Object, _registeredSensors);

        // Act
        bool overallResult = coordinator.VerifyEcosystemHealth();

        // Assert - 完全遵循 FluentAssertions 規範，禁用 xUnit 原生 Assert
        overallResult.Should().BeFalse();

        // 驗證呼叫次數，確保短路機制防線生效
        compromisedSensorMock.Verify(s => s.IsThreatDetected(), Times.Once);
        unexpectedSensorMock.Verify(s => s.IsThreatDetected(), Times.Never);
    }

}