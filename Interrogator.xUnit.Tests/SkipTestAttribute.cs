namespace Interrogator.xUnit.Tests
{
	public class SkipTestAttribute : IntegrationTestAttribute
	{
		public SkipTestAttribute()
		{
			Skip = "Skip This Test";
		}
	}
}