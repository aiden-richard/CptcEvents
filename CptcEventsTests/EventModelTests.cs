using Microsoft.VisualStudio.TestTools.UnitTesting;
using CptcEvents.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CptcEventsTests
{
    /// <summary>
    /// Unit tests for the <see cref="Event"/> model's validation logic.
    /// Tests cover time validation rules for timed and all-day events.
    /// </summary>
    [TestClass]
    public class EventModelTests
    {
        /// <summary>
        /// Verifies that a timed event with an end time before or equal to the start time
        /// produces a validation error.
        /// </summary>
        [TestMethod]
        public void Validate_TimedEvent_EndNotAfterStart_ReturnsValidationError()
        {
            // Arrange
            var e = new Event
            {
                Id = 1,
                Title = "Timed",
                IsAllDay = false,
                DateOfEvent = new DateOnly(2025, 1, 1),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(9, 0)
            };

            var context = new ValidationContext(e);

            // Act
            var results = e.Validate(context).ToList();

            // Assert
            Assert.IsTrue(results.Any(), "Expected validation errors when end time is not after start time for timed events.");
            Assert.IsTrue(results.Any(r => (r.MemberNames?.Contains(nameof(Event.EndTime)) ?? false) || (r.MemberNames?.Contains(nameof(Event.StartTime)) ?? false)), "Expected validation error to reference StartTime/EndTime.");
        }

        /// <summary>
        /// Verifies that a timed event with an end time after the start time
        /// passes validation without errors.
        /// </summary>
        [TestMethod]
        public void Validate_TimedEvent_EndAfterStart_NoValidationErrors()
        {
            // Arrange
            var e = new Event
            {
                Id = 2,
                Title = "Timed",
                IsAllDay = false,
                DateOfEvent = new DateOnly(2025, 1, 2),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            };

            var context = new ValidationContext(e);

            // Act
            var results = e.Validate(context).ToList();

            // Assert
            Assert.IsFalse(results.Any(), "Did not expect validation errors when end time is after start time.");
        }

        /// <summary>
        /// Verifies that all-day events ignore start/end time validation,
        /// even when the times would be invalid for a timed event.
        /// </summary>
        [TestMethod]
        public void Validate_AllDayEvent_IgnoresTimes_NoValidationErrors()
        {
            // Arrange
            var e = new Event
            {
                Id = 3,
                Title = "All Day",
                IsAllDay = true,
                DateOfEvent = new DateOnly(2025, 1, 3),
                StartTime = new TimeOnly(12, 0),
                EndTime = new TimeOnly(12, 0)
            };

            var context = new ValidationContext(e);

            // Act
            var results = e.Validate(context).ToList();

            // Assert
            Assert.IsFalse(results.Any(), "All-day events should not produce timing validation errors.");
        }
    }
}
