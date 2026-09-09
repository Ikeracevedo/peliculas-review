import { Injectable, Logger, OnModuleDestroy, OnModuleInit } from "@nestjs/common";
import { ConfigService } from "@nestjs/config";
import { PrismaLibSql } from '@prisma/adapter-libsql';
import { createClient } from '@libsql/client';
import { PrismaClient } from '../generated/prisma/client';

@Injectable()
export class PrismaService extends PrismaClient implements OnModuleInit, OnModuleDestroy
{
    private readonly logger = new Logger(PrismaService.name);

    constructor(configService: ConfigService)
    {
        const adapter = new PrismaLibSql({ url: 'file:./dev.db' });
        super({ adapter });
    }

    async onModuleInit() {
        await this.$connect();
        this.logger.log('Conexión a SQLite establecida');
    }

    async onModuleDestroy() {
        await this.$disconnect();
        this.logger.log('Conexión a SQLite cerrada');
    }
}