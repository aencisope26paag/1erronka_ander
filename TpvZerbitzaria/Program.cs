using TpvZerbitzaria;

DatuBasea db = new DatuBasea();
db.Hasieratu();
Console.WriteLine("Datu-basea prest: " + db.FitxategiBidea);

Zerbitzaria zerbitzaria = new Zerbitzaria(db);
zerbitzaria.Hasi();