// Script de la base de datos de PetMate (entregable del .rar, requisito 20 de los lineamientos).
// Crea las colecciones y los índices que hoy usa el backend (mismos nombres que MongoIndexInitializer).
// Es idempotente: se puede correr varias veces. No inserta datos reales.
//
// Uso (mongosh):
//   mongosh "<MONGODB_URI>" --file scripts/mongo/petmate-db.mongodb.js
// La base se toma de la variable de entorno PETMATE_DB (por defecto petmate_prod).

const dbName = (typeof process !== "undefined" && process.env.PETMATE_DB) || "petmate_prod";
const target = db.getSiblingDB(dbName);
print(`Base de datos: ${dbName}`);

function ensureCollection(name, validator) {
  const exists = target.getCollectionNames().includes(name);
  if (!exists) {
    target.createCollection(name, { validator, validationLevel: "moderate", validationAction: "error" });
    print(`  colección creada: ${name}`);
  } else {
    target.runCommand({ collMod: name, validator, validationLevel: "moderate", validationAction: "error" });
    print(`  colección existente (validador actualizado): ${name}`);
  }
}

// users: cuentas Owner/Business. role: 0 = Owner, 1 = Business (UserRole). Correo normalizado y único.
ensureCollection("users", {
  $jsonSchema: {
    bsonType: "object",
    required: ["_id", "email", "passwordHash", "role", "createdAtUtc", "updatedAtUtc"],
    properties: {
      _id: { bsonType: "binData", description: "Guid (UUID estándar)" },
      email: { bsonType: "string", description: "correo normalizado" },
      passwordHash: { bsonType: "string", description: "Argon2id en formato PHC" },
      role: { bsonType: "int", minimum: 0, maximum: 1 },
      createdAtUtc: { bsonType: "date" },
      updatedAtUtc: { bsonType: "date" },
    },
  },
});

// refresh_sessions: una sesión por inicio de sesión; vence por inactividad (30 días) o absoluto (90 días).
ensureCollection("refresh_sessions", {
  $jsonSchema: {
    bsonType: "object",
    required: ["_id", "userId", "createdAtUtc", "lastUsedAtUtc", "idleExpiresAtUtc", "absoluteExpiresAtUtc"],
    properties: {
      _id: { bsonType: "binData" },
      userId: { bsonType: "binData" },
      createdAtUtc: { bsonType: "date" },
      lastUsedAtUtc: { bsonType: "date" },
      idleExpiresAtUtc: { bsonType: "date" },
      absoluteExpiresAtUtc: { bsonType: "date" },
      revokedAtUtc: { bsonType: ["date", "null"] },
      revocationReason: { bsonType: ["int", "null"] },
    },
  },
});

// refresh_tokens: solo el hash del refresh token (nunca el token), de un solo uso, con rotación.
ensureCollection("refresh_tokens", {
  $jsonSchema: {
    bsonType: "object",
    required: ["_id", "sessionId", "tokenHash", "issuedAtUtc", "absoluteExpiresAtUtc"],
    properties: {
      _id: { bsonType: "binData" },
      sessionId: { bsonType: "binData" },
      tokenHash: { bsonType: "string" },
      issuedAtUtc: { bsonType: "date" },
      consumedAtUtc: { bsonType: ["date", "null"] },
      absoluteExpiresAtUtc: { bsonType: "date" },
    },
  },
});

// Índices (idénticos a src/PetMate.Infrastructure/Persistence/MongoIndexInitializer.cs).
target.users.createIndex({ email: 1 }, { unique: true, name: "ux_users_email" });
target.refresh_tokens.createIndex({ tokenHash: 1 }, { unique: true, name: "ux_refresh_tokens_tokenHash" });
target.refresh_tokens.createIndex({ sessionId: 1 }, { name: "ix_refresh_tokens_sessionId" });
target.refresh_tokens.createIndex({ absoluteExpiresAtUtc: 1 }, { expireAfterSeconds: 0, name: "ttl_refresh_tokens_absoluteExpiresAtUtc" });
target.refresh_sessions.createIndex({ userId: 1 }, { name: "ix_refresh_sessions_userId" });
target.refresh_sessions.createIndex({ absoluteExpiresAtUtc: 1 }, { expireAfterSeconds: 0, name: "ttl_refresh_sessions_absoluteExpiresAtUtc" });

print("Índices listos:");
for (const name of ["users", "refresh_sessions", "refresh_tokens"]) {
  print(`  ${name}: ${target[name].getIndexes().map((i) => i.name).join(", ")}`);
}
