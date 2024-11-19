<?php

namespace App\Models;

use Eloquent;
use App\Builders\Builder;
use App\Exceptions\Api\NotFoundException;
use Illuminate\Database\Eloquent\Concerns\HasAttributes;
use Illuminate\Database\Eloquent\Model as EloquentModel;
use Illuminate\Database\Eloquent\ModelNotFoundException;

/**
 * @mixin Eloquent
 * @mixin Builder
 */
class Model extends EloquentModel
{
    use HasAttributes;

    public static function findOrFailCustom(...$args)
    {
        try {
            return parent::findOrFail(...$args);
        }
        catch (ModelNotFoundException) {
            throw new NotFoundException(static::class);
        }
    }

    public function newEloquentBuilder($query)
    {
        return new Builder($query);
    }

    public function resolveRouteBinding($value, $field = null)
    {
        $model = parent::resolveRouteBinding($value, $field);

        if (!$model)
            throw new NotFoundException($this->class);

        return $model;
    }
}
