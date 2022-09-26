import React, {useEffect} from 'react';
import { Course } from '../models/course';
import {Card, Col, Radio, Row} from "antd";
import ShowCourses from '../components/ShowCourses';
import { useAppDispatch, useAppSelector } from '../redux/store/ConfigureStore';
import { coursesSelector, getCoursesAsync } from '../redux/slice/courseSlice';
import { categoriesSelector } from '../redux/slice/categorySlice';
import { Category } from '../models/category';

const sortOptions= [
  {value: "title", label: "Alphabetical"},
  {value: "priceDescending", label: "Price - High to Low"},
  {value: "priceAscending", label: "Price - Low to High"},

];


const HomePage = () => {
    const courses = useAppSelector(coursesSelector.selectAll);
    const dispatch = useAppDispatch();
    const {coursesLoaded} = useAppSelector((state) => state.course);
    
    useEffect(() => {
        if (!coursesLoaded) dispatch(getCoursesAsync())
    },[coursesLoaded, dispatch]);

    const categories = useAppSelector(categoriesSelector.selectAll)

    const getCategories = () => {
      const catArray:any[] = [];
      categories.forEach((category: Category) => {
        catArray.push({value: category.id, label: category.name});
      });
      return catArray;
    };
  return (
    <div className='course'>
        <div className="course__header" >
            <h1>What to learn next?</h1>
            <h2>New courses picked just for you...</h2>
        </div>
        <Row gutter={[24,32]}>
            <Col span={4}>
              <Card title="Sorting Options">
                <Radio.Group options={sortOptions}/>
              </Card>
              <Card title="Chose Category">
                <Radio.Group options={getCategories()}/>
              </Card>
            </Col>
            <Col span={20}>
              <Row gutter={[24,32]}>
                {courses && 
                courses.map((course: Course, index: number) => {
                  return <ShowCourses key={index} course={course}/>
                  } )}
              </Row>
            </Col>
          </Row>
    </div>
  )
}

export default HomePage

