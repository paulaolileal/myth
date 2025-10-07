using Myth.Flow.Test.Models;
using Myth.Models;

namespace Myth.Flow.Test.Interfaces {

	public interface IValidationService {

		Result<TestDto> Validate( TestDto dto );
	}
}