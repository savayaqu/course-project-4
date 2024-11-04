<?php

namespace App\Models;

use App\Exceptions\Api\ApiException;
use Illuminate\Database\Eloquent\Model as EloquentModel;

class Model extends EloquentModel
{
    public static function findOrFail($id) {
        $model = static::find($id);
        if(!$model)
            throw new ApiException(class_basename(static::class) . ' not found', 404);
        return $model;
    }
}
