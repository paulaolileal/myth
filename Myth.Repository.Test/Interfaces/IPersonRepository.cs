using Myth.Interfaces.Repositories.Base;
using Myth.Repository.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Repository.Test.Interfaces {
	internal interface IPersonRepository: IReadWriteRepositoryAsync<Person> {
	}
}
