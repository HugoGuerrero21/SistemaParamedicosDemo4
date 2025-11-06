using Newtonsoft.Json;
using SistemaParamedicosDemo4.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Service
{
    internal class ProductoAPIService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public async Task<List<ProductoDto>> GetProductosAsync() 
        {
            var response = await _httpClient.GetAsync("https://localhost:7057/api/productos");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProductoDto>>(json);
            }
            return new List<ProductoDto>();
        }
    }
}
