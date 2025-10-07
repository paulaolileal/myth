using Myth.Flow.Test.Models;
using System.Threading.Tasks;

namespace Myth.Flow.Test.Interfaces {

	public interface IEventService {

		Task PublishAsync( TestDto dto );
	}
}