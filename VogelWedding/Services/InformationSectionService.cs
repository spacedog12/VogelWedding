namespace VogelWedding.Services;

public class InformationSectionService
{
	public int _selectedSection = 1;
	public event Action<int> OnSelectionChange;

	public int SelectedSection
	{
		get => _selectedSection;
		set
		{
			_selectedSection = value;
			OnSelectionChange?.Invoke(value);
		}
	}
}
