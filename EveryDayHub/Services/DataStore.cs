using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

namespace EveryDayHub.Services
{
    // Generisk klass med platshållare som berättar att T måste vara en klass
    public class DataStore<T> where T : class
    {
        private readonly List<T> _items = new List<T>();
        private readonly string _filePath;

        // Konstruktor
        public DataStore(string filePath)
        {
            _filePath = filePath;
        }

        // Senaste felmeddelandet från spara eller ladda om allt gick bra är det tomt
        public string LastError { get; private set; } = "";

        // Lägger till ett objekt i listan
        public void AddItem(T item)
        {
            _items.Add(item);
        }

        // Ta bort ett objekt och returnerar false om objektet saknas
        public bool RemoveItem(T item)
        {
            return _items.Remove(item);
        }

        // Söker efter objekt och returnerar första matchningen och returnerar null om inget matchar
        public T? FindItem(Predicate<T> match)
        {
            return _items.Find(match);
        }

        // Returnerar en kopia av listan
        public List<T> GetAll()
        {
            return new List<T>(_items);
        }

        // Sparar lista som JSON i fil och returnerar true om allt gick bra
        public bool SaveToJson()
        {
            try
            {
                string json = JsonSerializer.Serialize(_items);
                File.WriteAllText(_filePath, json);
                LastError = "";
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                LastError = $"Saknar behörighet att spara filen {_filePath}.";
                return false;
            }
            catch (IOException ex)
            {
                LastError = $"Kunde inte skriva till filen {_filePath}: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                LastError = $"Oväntat fel vid sparning: {ex.Message}";
                return false;
            }
        }

        // Laddar lista från JSON-fil och returnerar true om allt gick bra
        public bool LoadFromJson()
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                List<T>? loaded = JsonSerializer.Deserialize<List<T>>(json);

                if (loaded == null)
                {
                    LastError = $"Filen {_filePath} innhåller inga giltiga data.";
                    return false;
                }

                _items.Clear();
                _items.AddRange(loaded);
                LastError = "";
                return true;
            }
            catch (FileNotFoundException)
            {
                LastError = $"Filen {_filePath} hittades inte.";
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                LastError = $"Saknar behörighet att läsa filen {_filePath}.";
                return false;
            }
            catch (IOException ex)
            {
                LastError = $"Kunde inte läsa filen {_filePath}: {ex.Message}";
                return false;
            }
            catch (JsonException ex)
            {
                LastError = $"Filen {_filePath} har ogiltigt JSON-format: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                LastError = $"Oväntat fel vid inläsning: {ex.Message}";
                return false;
            }
        }
    }
}