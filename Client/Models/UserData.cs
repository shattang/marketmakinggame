using System;
using System.ComponentModel.DataAnnotations;

namespace MarketMakingGame.Client.Models
{
  public class UserData
  {
    private string displayName;
    private string userId;
    private string avatarSeed;

    [Required]
    [MaxLength(20, ErrorMessage = "Max 20 characters")]
    [MinLength(3, ErrorMessage = "Min 3 characters")]
    public String DisplayName
    {
      get => displayName;
      set { displayName = value; InvokeOnChanged(); }
    }

    public String UserId
    {
      get => userId;
      set { userId = value; InvokeOnChanged(); }
    }

    public String AvatarSeed
    {
      get => avatarSeed;
      set { avatarSeed = value; InvokeOnChanged(); }
    }

    public override string ToString()
    {
      return $"UserData(UserId={UserId}, DisplayName={DisplayName}, AvatarSeed={AvatarSeed})";
    }

    private void InvokeOnChanged()
    {
      if (OnChanged != null)
        OnChanged(EventArgs.Empty);
    }

    public event Action<EventArgs> OnChanged;
  }
}