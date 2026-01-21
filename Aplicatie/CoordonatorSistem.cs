using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ConsoleApp5
{
    public sealed class CoordonatorSistem
    {
        private readonly ILogger<CoordonatorSistem> _logger;

        public CoordonatorSistem(ILogger<CoordonatorSistem> logger)
        {
            _logger = logger;
        }

        public bool TryAdaugaProdus(Matcherie matcherie, Matcha produs, out string mesaj)
        {
            mesaj = "";

            if (matcherie == null) { mesaj = "Matchery is null."; return false; }
            if (produs == null || string.IsNullOrWhiteSpace(produs.Nume)) { mesaj = "Invalid product (missing name)."; return false; }
            if (matcherie.Meniu == null) { mesaj = "Matchery menu is not initialized."; return false; }

            bool exista = matcherie.Meniu.Any(p =>
                string.Equals(p.Nume, produs.Nume, StringComparison.OrdinalIgnoreCase));

            if (exista)
            {
                mesaj = "A product with this name already exists in the menu.";
                return false;
            }

            matcherie.Meniu.Add(produs);

            _logger.LogInformation("Admin added product | Matchery={Matchery} | Product={Product} | Price={Price}",
                matcherie.Nume, produs.Nume, produs.Pret);

            mesaj = "Product added successfully.";
            return true;
        }

        public bool TryCreeazaRezervare(
            ContClient client,
            Matcherie matcherie,
            TipRezervare tip,
            out Rezervare? rezervareNoua,
            out string mesaj)
        {
            rezervareNoua = null;
            mesaj = "";

            if (client == null) { mesaj = "Client is null."; return false; }
            if (matcherie == null) { mesaj = "Matchery is null."; return false; }
            if (tip == null) { mesaj = "Reservation type is null."; return false; }

            int cap = matcherie.Capacitate <= 0 ? 0 : matcherie.Capacitate;
            int ocupate = matcherie.Rezervari?.Count ?? 0;

            if (cap == 0) { mesaj = "Matchery capacity is invalid (0)."; return false; }
            if (ocupate >= cap) { mesaj = "Sorry, the matchery is full."; return false; }

            bool existaDeja = client.Rezervari.Any(r =>
                string.Equals(r.MatcherieNume ?? "", matcherie.Nume ?? "", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.Tip ?? "", tip.Nume ?? "", StringComparison.OrdinalIgnoreCase));

            if (existaDeja)
            {
                mesaj = "You already have a reservation of this type for this matchery.";
                return false;
            }

            rezervareNoua = new Rezervare(
                tip.Nume,
                tip.Pret,
                tip.Limitari,
                tip.Beneficii,
                client.Nume,
                matcherie
            );

            matcherie.Rezervari.Add(rezervareNoua);
            client.Rezervari.Add(rezervareNoua);

            _logger.LogInformation("Client created reservation | Client={Client} | Matchery={Matchery} | Type={Type} | Price={Price}",
                client.Email, matcherie.Nume, tip.Nume, tip.Pret);

            mesaj = "Reservation created successfully.";
            return true;
        }

        public bool TryAnuleazaRezervare(ContClient client, Rezervare rezervare, out string mesaj)
        {
            mesaj = "";

            if (client == null) { mesaj = "Client is null."; return false; }
            if (rezervare == null) { mesaj = "Reservation is null."; return false; }

            var matcherie = rezervare.Matcherie;
            if (matcherie == null && !string.IsNullOrWhiteSpace(rezervare.MatcherieNume))
            {
                matcherie = null;
            }

            bool removedFromMatcherie = false;

            if (matcherie != null)
                removedFromMatcherie = matcherie.Rezervari.Remove(rezervare);
            else
                removedFromMatcherie = false;

            bool removedFromClient = client.Rezervari.Remove(rezervare);

            if (!removedFromMatcherie && !removedFromClient)
            {
                mesaj = "Reservation not found for cancellation.";
                return false;
            }

            _logger.LogInformation("Client canceled reservation | Client={Client} | Matchery={Matchery} | Type={Type} | Price={Price}",
                client.Email, rezervare.MatcherieNume, rezervare.Tip, rezervare.Pret);

            mesaj = "Reservation canceled successfully.";
            return true;
        }
    }
}
