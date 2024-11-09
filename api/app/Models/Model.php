<?php

namespace App\Models;

use App\Builders\Builder;
use App\Exceptions\Api\ApiException;
use App\Exceptions\Api\NotFoundException;
use Illuminate\Database\Eloquent\Model as EloquentModel;
use Illuminate\Database\Eloquent\ModelNotFoundException;

class Model extends EloquentModel
{
    public static function findOrFailCustom(...$args)
    {
        try {
            return parent::findOrFail(...$args);
        }
        catch(\Exception $e) {
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

        if (!$model) {
            throw new NotFoundException($this::class);
        }

        return $model;
    }
}
