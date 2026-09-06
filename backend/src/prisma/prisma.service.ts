import { Injectable, Logger, OnModuleDestroy, OnModuleInit } from "@nestjs/common";
import { ConfigService } from "@nestjs/config";
import { PrismaBetterSqlite3 } from '@prisma/adapter-better-sqlite3';
import { PrismaClient } from '../generated/prisma/client';


/**
 * Envuelve al PrismaClient como provider de Nest para que el contenedor
 * Gestione sus ciclo de vida autonomamente
 */
@Injectable()
export class PrismaService extends PrismaClient implements OnModuleInit, OnModuleDestroy
{
    
    private readonly logger = new Logger(PrismaService.name);

    constructor(configService: ConfigService)
    {
        const url = configService.getOrThrow<string>('DATABASE_URL');
        super({ adapter: new PrismaBetterSqlite3({ url }) });
    }

    /**
     * Ciclos de vida del proyecto de nest
     */

    async onModuleInit() {
        await this.$connect();
        this.logger.log('Conexión a SQLite establecida');
    }

    async onModuleDestroy() {
        await this.$disconnect();
        this.logger.log('Conexión a SQLite cerrada');
    }
}