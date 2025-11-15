using System.Threading.Tasks;
using Myth.Flow.Test.Models;
using Myth.Models;

namespace Myth.Flow.Test.Interfaces;

public interface ITestService {

	TestDto Process( TestDto dto );

	Task<TestDto> ProcessAsync( TestDto dto );

	Result<TestDto> ProcessWithResult( TestDto dto );

	Task<Result<TestDto>> ProcessWithResultAsync( TestDto dto );
}
