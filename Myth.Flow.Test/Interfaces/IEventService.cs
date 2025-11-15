using System.Threading.Tasks;
using Myth.Flow.Test.Models;

namespace Myth.Flow.Test.Interfaces;

public interface IEventService {

	Task PublishAsync( TestDto dto );
}
