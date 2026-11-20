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

        // Ångra och gör om. Varje version av listan sparas som JSON-text i en stack, där den senaste ligger överst.
        private readonly Stack<string> _undoStack = new Stack<string>();
        private readonly Stack<string> _redoStack = new Stack<string>();

        // Listan som den såg ut senast den sparades eller laddades. "[ ]" betyder en tom lista.
        private string _savedJson = "[]";

        // Konstruktor
        public DataStore(string filePath)
        {
            _filePath = filePath;
        }

        // Senaste felmeddelandet från spara eller ladda om allt gick bra är det tomt
        public string LastError { get; private set; } = "";

        // True om det finns något att ångra eller göra om. => är ett kort sätt att skrive en egenskap som bara läses.
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

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

                // Den förra versionen läggs på ångra-stacken, men bara om listan faktiskt har ändrats.
                // En ny ändring gör att det som ändrats inte längra kan göras om, därför töms gör om-stacken.
                if (json != _savedJson)
                {
                    _undoStack.Push(_savedJson);
                    _redoStack.Clear();
                    _savedJson = json;
                }

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

                // Utgångsläget för ångra. Texten görs om med Serialize, så att den går att jämföra med texten i SaveToJson.
                _savedJson = JsonSerializer.Serialize(_items);
                _undoStack.Clear();
                _redoStack.Clear();

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

        // Går tillbaka till versionen för den senaste sparade ändringen. Returnerar true om det gick bra.
        public bool Undo()
        {
            if (_undoStack.Count == 0)
            {
                LastError = "Det finns inget att ångra.";
                return false;
            }

            // Nuvarande version flyttas till gör om-stacken och den förra versionen hämtas från ångra-stacken.
            _redoStack.Push(_savedJson);
            return Restore(_undoStack.Pop());
        }

        // Gör om den senast ångrade ändringen, returnera true om det gick bra.
        public bool Redo()
        {
            if (_redoStack.Count == 0)
            {
                LastError = "Det finns inget att göra om.";
                return false;
            }

            // Samma sak åt andra hållet: nuvarande version till ångra-stacken, nästa hämtas från gör om-stacken.
            _undoStack.Push(_savedJson);
            return Restore(_redoStack.Pop());
        }

        // Byter ut listan mot en sparad version och skriver den till filen.
        private bool Restore(string json)
        {
            // Om resultatet blir null betyder "??" att en tom lista kommer användas.
            _items.Clear();
            _items.AddRange(JsonSerializer.Deserialize<List<T>> (json) ?? new List<T>());
            _savedJson = json;

            try
            {
                File.WriteAllText(_filePath, json);
                LastError = "";
                return true;
            }
            catch(Exception ex)
            {
                LastError = $"Kunde inte spara filen {_filePath}: {ex.Message}";
                return false;
            }
        }
    }
}