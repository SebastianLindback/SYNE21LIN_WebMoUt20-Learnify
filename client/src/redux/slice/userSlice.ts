import { createAsyncThunk, createSlice, isAnyOf } from "@reduxjs/toolkit";
import agent from "../../actions/agent";
import { Login, Register, User } from "../../models/user";
import { setBasket } from "./basketSlice";

interface userState {
    user:User | null
}

const initalState : userState = {
    user : null,
}

export const signInUser = createAsyncThunk<User, Login>(
    "user/signin",
    async (data, thunkAPI) => {
      try {
        const userData = await agent.Users.login(data);
        const {basket, ...user} = userData;
        if (basket) thunkAPI.dispatch(setBasket(basket));
        localStorage.setItem("user", JSON.stringify(user));
        return user;
      } catch (err: any) {
        return thunkAPI.rejectWithValue({ error: err });
      }
    }
  );
export const registerUser = createAsyncThunk<User, Register>(
    "user/register",
    async (data, trunkAPI) => {
    try {
        const user = await agent.Users.login(data);
        localStorage.setItem("user", JSON.stringify(user));
        return user;

    } catch (error) {
        return trunkAPI.rejectWithValue({error: error})
    }
   }
);

export const userSlice = createSlice({
    name: "user",
    initialState : initalState,
    reducers: {
        signOut: (state) => {
            state.user = null;
            localStorage.removeItem("user")
        },
        getUser: (state) => {
          const user = localStorage.getItem("user");
          if (user){
            state.user = JSON.parse(user);
          }
        }
    },
    extraReducers: (builder) => {
        builder.addMatcher(
            isAnyOf(signInUser.fulfilled, registerUser.fulfilled),
            (state, action) => {
              state.user = action.payload;
            }
          );
           builder.addMatcher(
            isAnyOf(signInUser.rejected, registerUser.rejected),
            (state, action) => {
              throw action.payload;      }
          );
    },
})

export const {signOut, getUser} = userSlice.actions;