// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Application\Common\Exceptions\NotFoundException.cs

namespace Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
