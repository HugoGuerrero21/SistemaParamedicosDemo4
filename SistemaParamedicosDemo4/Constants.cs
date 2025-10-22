using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4
{
    public class Constants
    {
        //Declaro uns constante de tipo string la cual sera el archivo que guarde en memoria para almacenar mis datos mientras no tenga conexión
        private const string DBFilName = "SQLite.db3";
        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        public static string DataBasePath
        {
            get
            {
                return Path.Combine(FileSystem.AppDataDirectory, DBFilName);
                //Accedemos a la carpeta de dirección de cada sistema de plataforma donde pasaremos el contexto de nuestro datos guardados temporalmente en SQLite 
            }
        }
    }
}
