using Newtonsoft.Json;
using PicsyncAdmin.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public int? Status { get; set; }
        public string? Description { get; set; }
        public required User  AboutUser { get; set; }
        public User? FromUser { get; set; }
        public string? Sign { get; set; }
        public Picture? Picture { get; set; }
        public Album? Album { get; set; }
        // Свойство для получения описания статуса
        public string StatusDescription
        {
            get
            {
                return Status switch
                {
                    1 => "Рассмотрена",
                    0 => "Отклонена",
                    _ => "Не рассмотрена" // Для null (или других значений, если status не инициализирован)
                };
            }
        }
    }
}
