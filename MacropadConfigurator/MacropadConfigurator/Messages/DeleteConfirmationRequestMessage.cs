using CommunityToolkit.Mvvm.Messaging.Messages;
using MahApps.Metro.Controls.Dialogs;

namespace MacropadConfigurator.Messages;

public  class DeleteConfirmationRequestMessage(string name) : AsyncRequestMessage<MessageDialogResult>
{
    public string Name { get; init; } = name;
}
