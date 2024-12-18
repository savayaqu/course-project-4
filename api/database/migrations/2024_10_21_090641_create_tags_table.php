<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('tags', function (Blueprint $table) {
            $table->id();
            $table->string   ('value');
            $table->foreignId('user_id')->constrained();
            $table->timestamps();

            $table->unique(['value', 'user_id']);
        });

        Schema::create('tag_pictures', function (Blueprint $table) {
            $table->primary(['tag_id', 'picture_id']);
            $table->foreignId('tag_id'    )->constrained()->cascadeOnDelete();
            $table->foreignId('picture_id')->constrained()->cascadeOnDelete();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('tags');
    }
};
