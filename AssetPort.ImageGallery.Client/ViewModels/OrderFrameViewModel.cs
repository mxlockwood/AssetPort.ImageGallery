using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetPort.ImageGallery.Client.ViewModels
{
    public class OrderFrameViewModel
    {
        public string Address { get; private set; } = string.Empty;

        public string Role { get; private set; } = string.Empty;

        public OrderFrameViewModel(string address, string role)
        {
            Address = address;
            Role = role;
        }
    }
}
