using FluentAssertions;
using NSubstitute;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagment.Tests.Models
{
    public class MessageEnvelopTests
    {
        [Fact]
        public void State_WhenStateIsNotSet_ThenDefaultsToNew()
        {
            // Arrange
            var envelop = new MessageEnvelop() { EntityId = "123" };

            // Act
            var state = envelop.State;

            // Assert
            state.Should().Be(EntityState.NEW);
        }

        [Fact]
        public void State_WhenStateIsSet_ThenReturnsSetValue()
        {
            // Arrange
            var envelop = new MessageEnvelop() { EntityId = "123" };

            // Act
            envelop.SetState(EntityState.EVALUATING);

            // Assert
            envelop.State.Should().Be(EntityState.EVALUATING);
        }
    }
}
