using System.Threading.Tasks;

namespace Interrogator.xUnit.Tests
{
	public class Stuff
	{
		public async Task<string> Message()
		{
			await Task.Delay(500);

			return "abc123";
		}
	}
}